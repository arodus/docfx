// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;
using Mono.Cecil;
using Nuke.Common;

namespace Nuke.DocFX.Generator.Utilities
{
    internal static class PropertyDefinitionExtensions
    {
        private static readonly string[] s_argumentAttributeTypeFullName =
            { "CommandLine.ValueOptionAttribute", "CommandLine.OptionAttribute", "CommandLine.OptionListAttribute" };

        public static CustomAttribute GetArgumentAttribute(this PropertyDefinition propertyDefinition)
        {
            ControlFlow.Assert(propertyDefinition.HasArgumentAttribute(), "typeDefinition.HasArgumentAttribute()");
            return propertyDefinition.CustomAttributes.Single(x => s_argumentAttributeTypeFullName.Any(y => x.AttributeType.FullName == y));
        }

        public static bool HasArgumentAttribute(this PropertyDefinition propertyDefinition)
        {
            return propertyDefinition.HasCustomAttributes
                   && propertyDefinition.CustomAttributes.Any(x => s_argumentAttributeTypeFullName.Any(y => x.AttributeType.FullName == y));
        }

        public static string GetTypeName(this PropertyDefinition propertyDefinition, CustomAttribute argumentAttribute)
        {
            var propertyType = propertyDefinition.PropertyType;
            if (propertyType.IsGenericInstance)
            {
                var instanceType = (GenericInstanceType) propertyType;
                var genericType = instanceType.GenericArguments.Single();
                var genericTypeName = GetNukeTypeName(genericType, out var isPrimitive);
                if (propertyType.Name.StartsWith("Nullable`"))
                    return genericTypeName + (isPrimitive || genericType.Resolve().IsEnum ? string.Empty : "?");
                if (propertyType.Name.StartsWith("List`")) return $"List<{genericTypeName}>";
            }

            return GetNukeTypeName(propertyDefinition.PropertyType, out _);

            string GetNukeTypeName(TypeReference type, out bool isPrimitive)
            {
                isPrimitive = true;
                switch (type.Name)
                {
                    case "Boolean":
                        return "bool";
                    case "String":
                    case "Object":
                        return "string";
                    case "Int32":
                        return "int";
                    case "ListWithStringFallback":
                        return "List<string>";
                    default:
                    {
                        isPrimitive = false;
                        return type.Name;
                    }
                }
            }
        }
    }
}
