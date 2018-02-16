// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;

namespace Nuke.DocFX.Generator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DocFXSpecificationGenerator.WriteSpecifications(x =>
                x.SetOutputFolder(args[0])
                    .SetPackageFolder(args[1])
                    .SetGitReference(args[2])
                    .SetOverwriteFile(args[3]))
                ;
        }
    }
}
