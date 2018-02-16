// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;

namespace Nuke.DocFX.Generator
{
    public class DocFXSpecificationGeneratorSettings : SpecificationGeneratorSettings
    {
        public string PackageFolder { get; set; }
        public string GitReference { get; set; }
        public string OverwriteFile { get; set; }
    }
}
