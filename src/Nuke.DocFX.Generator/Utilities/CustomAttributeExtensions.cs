// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Nuke.DocFX.Generator.Utilities
{
    internal static class CustomAttributeExtensions
    {
        [CanBeNull]
        public static T GetPropertyValue<T>(this CustomAttribute attribute, string propertyName)
        {
            if (!attribute.HasProperties || attribute.Properties.All(x => x.Name != propertyName)) return default(T);
            return (T) attribute.Properties.Single(x => x.Name == propertyName).Argument.Value;
        }
    }
}
