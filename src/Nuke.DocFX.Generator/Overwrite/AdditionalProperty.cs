// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;
using Nuke.CodeGeneration.Model;

namespace Nuke.Helm.Generator.Overwrite
{
    internal class AdditionalProperty : Property
    {
        public Position Position { get; set; }
    }
}
