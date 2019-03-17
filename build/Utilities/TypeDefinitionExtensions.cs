// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Nuke.Common;

namespace Nuke.DocFX.Generator.Utilities
{
    internal static class TypeDefinitionExtensions
    {
        private const string c_commandAttributeTypeFullName = "Microsoft.DocAsCode.OptionUsageAttribute";
        private const string c_commandOptionAttributeTypeFullName = "Microsoft.DocAsCode.SubCommands.CommandOptionAttribute";


        public static CustomAttribute[] GetCommandAttributes(this TypeDefinition typeDefinition)
        {
            return typeDefinition.GetAttributes(c_commandAttributeTypeFullName);
        }

        private static CustomAttribute[] GetAttributes(this TypeDefinition typeDefinition, string attributeTypeFullName)
        {
            ControlFlow.Assert(typeDefinition.HasAttribute(attributeTypeFullName), "typeDefinition.HasCommandAttribute()");
            return typeDefinition.CustomAttributes.Where(x => x.AttributeType.FullName == attributeTypeFullName).ToArray();
        }

        private static bool HasAttribute(this TypeDefinition typeDefinition, string attributeTypeFullName)
        {
            return typeDefinition.HasCustomAttributes
                   && typeDefinition.CustomAttributes.Any(x => x.AttributeType.FullName == attributeTypeFullName);
        }

        public static bool HasCommandAttribute(this TypeDefinition typeDefinition)
        {
            return typeDefinition.HasAttribute(c_commandAttributeTypeFullName);
        }

        [CanBeNull]
        public static CustomAttribute GetCommandOptionAttribute(this TypeDefinition typeDefinition)
        {
            return !typeDefinition.HasAttribute(c_commandOptionAttributeTypeFullName) ? null : typeDefinition.GetAttributes(c_commandOptionAttributeTypeFullName).Single();
        }

        public static bool HasCommandOptions(this TypeDefinition typeDefinition)
        {
            return typeDefinition.Properties.Any(x => x.HasArgumentAttribute());
        }

        public static TypeDefinition[] GetInheritedTypesWithOptions(this TypeDefinition definition)
        {
            var baseType = definition.BaseType?.Resolve();
            if (baseType == null) return new TypeDefinition[0];

            var baseTypes = new List<TypeDefinition>();
            if (baseType.HasCommandOptions()) baseTypes.Add(baseType);
            baseTypes.AddRange(GetInheritedTypesWithOptions(baseType));
            return baseTypes.ToArray();
        }
    }
}
