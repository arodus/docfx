// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.CodeGeneration.Model;

namespace Nuke.DocFX.Generator.Utilities
{
    [Serializable]
    internal class EnumerationEqualityComparer : IEqualityComparer<Enumeration>
    {
        public bool Equals(Enumeration x, Enumeration y)
        {
            return x.Name == y.Name && new SequenceEqualityComparer().Equals(x.Values, y.Values);
        }

        public int GetHashCode(Enumeration obj)
        {
            var result = 17;

            unchecked
            {
                result = result * 23 + obj.Name.GetHashCode();
                result = result * 23 + new SequenceEqualityComparer().GetHashCode(obj.Values);
            }

            return result;
        }
    }
}
