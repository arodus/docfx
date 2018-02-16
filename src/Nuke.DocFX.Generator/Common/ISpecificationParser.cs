// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;
using Nuke.CodeGeneration.Model;

namespace Nuke.DocFX.Generator
{
    public interface ISpecificationParser : IDisposable
    {
        void Populate(Tool tool);
    }
}
