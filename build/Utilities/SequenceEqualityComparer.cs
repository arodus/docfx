// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nuke.DocFX.Generator.Utilities
{
    [Serializable]
    internal class SequenceEqualityComparer : IEqualityComparer<List<string>>
    {
        public bool Equals(List<string> x, List<string> y)
        {
            if (x.Count != y.Count) return false;
            for (var i = 0; i < x.Count; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        public int GetHashCode(List<string> obj)
        {
            var result = 17;

            for (var i = 0; i < obj.Count; i++)
            {
                unchecked
                {
                    result = result * 23 + i.GetHashCode();
                }
            }

            return result;
        }
    }
}
