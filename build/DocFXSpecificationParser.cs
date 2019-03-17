// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Nuke.CodeGeneration.Model;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.DocFX.Generator.Utilities;
using Nuke.Helm.Generator.Utilities;

namespace Nuke.DocFX.Generator
{
    public class DocFXSpecificationParser : SpecificationParser
    {
        public static DocFXSpecificationParser FromPackages(string packageDirectory, string gitReference)
        {
            var assemblies = LoadAssemblies(packageDirectory);
            return new DocFXSpecificationParser(assemblies, gitReference);
        }

        private static AssemblyDefinition[] LoadAssemblies(string packageDirectory)
        {
            var assemblyDirectory = PathConstruction.Combine(packageDirectory, "tools");

            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(assemblyDirectory);
            return PathConstruction.GlobFiles(assemblyDirectory, "docfx.exe")
                .Select(x => AssemblyDefinition.ReadAssembly(x, new ReaderParameters { AssemblyResolver = assemblyResolver }))
                .ToArray();
        }

        private readonly AssemblyDefinition[] _assemblies;
        private readonly string _gitReference;
        private readonly TypeDefinition[] _commandTypes;
        private readonly HashSet<TypeDefinition> _commonTaskPropertyTypes = new HashSet<TypeDefinition>();
        private readonly Dictionary<string, string> _commandHelpTexts;

        public DocFXSpecificationParser(AssemblyDefinition[] assemblies, string gitReference)
        {
            _assemblies = assemblies;
            _gitReference = gitReference;

            var types = assemblies
                .SelectMany(x => x.MainModule.Types)
                .ToArray();
            _commandTypes =
                types
                    .Where(x => x.HasCommandAttribute())
                    .ToArray();

            _commandHelpTexts = types
                .Select(x => x.GetCommandOptionAttribute())
                .Where(x => x != null)
                .ToDictionary(x => GetStringConstructorArgument(x, index: 0), x => GetStringConstructorArgument(x, index: 1).FormatForXmlDoc());

            string GetStringConstructorArgument(ICustomAttribute attribute, int index) => (string) attribute.ConstructorArguments[index].Value;
        }

        public override void Dispose()
        {
            foreach (var assemblyDefinition in _assemblies)
            {
                assemblyDefinition?.Dispose();
            }
        }

        protected override List<Task> ParseTasks(Tool tool)
        {
            return _commandTypes
                .Select(x => ParseTask(x, tool))
                .ToList();
        }

        protected override Dictionary<string, List<Property>> ParseCommonTaskPropertySets()
        {
            return _commonTaskPropertyTypes
                .ToDictionary(x => x.Name.ToLowerInvariant(), x => ParseProperties(x, dataClass: null));
        }

        protected override List<Enumeration> ParseEnumerations()
        {
            bool IsUnique(Enumeration[] grouping)
            {
                return grouping.Count() == 1 || grouping.GroupBy(x => x.Values, new SequenceEqualityComparer()).Count() == 1;
            }

            var argumentTypes = _assemblies
                .SelectMany(x => x.MainModule.Types)
                .SelectMany(x => x.Properties)
                .Where(x => x.HasArgumentAttribute())
                .Select(x => x.PropertyType).ToArray();

            var enumerations = argumentTypes
                .Where(x => x.IsGenericInstance && x.Name.StartsWith("Nullable`"))
                .Cast<GenericInstanceType>()
                .Select(x => x.GenericArguments.Single())
                .Concat(argumentTypes)
                .Select(x => x.Resolve())
                .Where(x => x.IsEnum)
                .Select(ParseEnumeration)
                .ToLookup(x => x.Name, x => x)
                .ToArray();

            var result = new List<Enumeration>();
            enumerations
                .ToList()
                .ForEach(x =>
                {
                    ControlFlow.Assert(IsUnique(x.ToArray()),
                        "Multiple enumerations with same name but different values were found.");

                    result.Add(x.First());
                });

            return result;
        }

        protected override void PostPopulate(Tool tool)
        {
            tool.CommonTaskPropertySets.SelectMany(x => x.Value)
                .Concat(tool.Tasks.SelectMany(x => x.SettingsClass.Properties))
                .Concat(tool.CommonTaskProperties)
                .ToList()
                .ForEach(x =>
                {
                    if (tool.Enumerations.Any(y => y.Name == x.Type))
                        x.Type = $"DocFX{x.Type}";
                });

            tool.Enumerations.ForEach(x => x.Name = $"DocFX{x.Name}");
        }

        private Enumeration ParseEnumeration(TypeDefinition typeDefinition)
        {
            return new Enumeration
                   {
                       Name = typeDefinition.Name,
                       Values = typeDefinition.Fields.Where(x => x.Name != "value__").Select(x => x.Name).ToList()
                   };
        }

        private Task ParseTask(TypeDefinition typeDefinition, Tool tool)
        {
            var commandAttributes = typeDefinition.GetCommandAttributes();
            var postfix = typeDefinition.Name.Replace("CommandOptions", string.Empty);

            var baseClasses = typeDefinition.GetInheritedTypesWithOptions().ToList();
            baseClasses.ForEach(x => _commonTaskPropertyTypes.Add(x));
            var definiteArgument = postfix.ToLowerInvariant();
            var task = new Task
                       {
                           Help = _commandHelpTexts.GetValueOrDefault(definiteArgument, defaultValue: null),
                           Tool = tool,
                           Postfix = postfix,
                           DefiniteArgument = definiteArgument,
                           CommonPropertySets = baseClasses.Select(x => x.Name.ToLowerInvariant()).ToList()
                       };
            var settingsClass = new SettingsClass
                                {
                                    Tool = tool,
                                    Task = task
                                };
            settingsClass.Properties = ParseProperties(typeDefinition, settingsClass);
            task.SettingsClass = settingsClass;
            return task;
        }

        private List<Property> ParseProperties(TypeDefinition typeDefinition, [CanBeNull] DataClass dataClass)
        {
            var positionalArguments = new Dictionary<string, int>();

            var properties = typeDefinition.Properties
                .Where(x => x.HasArgumentAttribute())
                .Select(x =>
                {
                    var argumentAttribute = x.GetArgumentAttribute();
                    var typeName = x.GetTypeName(argumentAttribute);

                    var name = x.Name;

                    var isRequired = argumentAttribute.GetPropertyValue<bool>("Required");
                    var separator = argumentAttribute.GetPropertyValue<char?>("Separator");
                    separator = separator.HasValue && separator.Value == '\0' ? null : separator;

                    string longName = null;
                    string shortName = null;
                    int? position = null;

                    var firstCtorArgument = argumentAttribute.ConstructorArguments.First();
                    if (argumentAttribute.ConstructorArguments.Count == 1)
                    {
                        var argumentTypeName = firstCtorArgument.Type.FullName;
                        switch (argumentTypeName)
                        {
                            case "System.Char":
                                shortName = (string) firstCtorArgument.Value;
                                break;
                            case "System.String":
                                longName = (string) firstCtorArgument.Value;
                                break;
                            case "System.Int32":
                                position = (int) firstCtorArgument.Value;
                                break;
                            default:
                                throw new NotImplementedException($"Constructor arguments of type {argumentTypeName} are not supported, yet.");
                        }
                    }
                    else
                    {
                        shortName = firstCtorArgument.Type.FullName == "System.Char"
                            ? ((char) firstCtorArgument.Value).ToString()
                            : (string) firstCtorArgument.Value;
                        longName = (string) argumentAttribute.ConstructorArguments[index: 1].Value;

                        if (argumentAttribute.ConstructorArguments.Count == 3)
                        {
                            separator = separator ?? (char) argumentAttribute.ConstructorArguments[index: 3].Value;
                        }
                    }

                    var format = BuildFormatString(shortName, longName, typeName != "bool");
                    if (position != null)
                    {
                        positionalArguments.Add(name, position.Value);
                        format = "{value}";
                    }

                    return new Property
                           {
                               Name = name,
                               Type = typeName,
                               Help = argumentAttribute.GetPropertyValue<string>("HelpText").FormatForXmlDoc(),
                               Format = format,
                               Separator = separator == default(char) || separator == '\0' ? default(char?) : separator,
                               ItemFormat = typeName.StartsWith("Dictionary<") ? "{key}={value}" : null,
                               DataClass = dataClass
                           };
                })
                .OrderBy(x => positionalArguments.TryGetValue(x.Name, out var position) ? $"!{position}" : x.Name)
                .ToList();
            return properties;
        }

        private static string BuildFormatString([CanBeNull] string shortName, [CanBeNull] string longName, bool includeValue)
        {
            var longFormat = longName != null;
            var name = longFormat ? longName : shortName;
            var prefix = longFormat ? "--" : "-";
            var separator = longFormat ? '=' : ':';

            var format = $"{prefix}{name}";
            if (includeValue)
                format += $"{separator}{{value}}";
            return format;
        }
    }
}
