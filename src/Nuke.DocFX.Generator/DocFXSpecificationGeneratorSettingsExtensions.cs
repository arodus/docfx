// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;

namespace Nuke.DocFX.Generator
{
    public static class DocFXSpecificationGeneratorSettingsExtensions
    {
        public static DocFXSpecificationGeneratorSettings SetPackageFolder(this DocFXSpecificationGeneratorSettings settings, string packageFolder)
        {
            settings.PackageFolder = packageFolder;
            return settings;
        }

        public static DocFXSpecificationGeneratorSettings SetGitReference(this DocFXSpecificationGeneratorSettings settings, string gitReference)
        {
            settings.GitReference = gitReference;
            return settings;
        }

        public static DocFXSpecificationGeneratorSettings SetOverwriteFile(
            this DocFXSpecificationGeneratorSettings settings,
            string overwriteFilePath)
        {
            settings.OverwriteFile = overwriteFilePath;
            return settings;
        }
    }
}
