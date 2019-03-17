// Copyright 2019 Maintainers and Contributors of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.NuGet;
using Nuke.DocFX.Generator;
using Nuke.GitHub;
using static Nuke.GitHub.GitHubTasks;

partial class Build
{
    PathConstruction.AbsolutePath DefinitionRepositoryPath => TemporaryDirectory / "definition-repository";

    Target Specifications => _ => _
        .DependentFor(Generate)
        .Executes(() =>
        {
            if (Directory.Exists(DefinitionRepositoryPath))
                FileSystemTasks.DeleteDirectory(DefinitionRepositoryPath);

            var packageId = "docfx.console";
            NuGetTasks.NuGet($"install {packageId} "
                             + $"-OutputDirectory {DefinitionRepositoryPath} "
                             + "-ExcludeVersion "
                             + "-DependencyVersion Ignore "
                             + "-Verbosity detailed");

            DocFXSpecificationGenerator.WriteSpecifications(x => x
                .SetOutputFolder(SourceDirectory / "Nuke.DocFX")
                .SetOverwriteFile(SourceDirectory / "Nuke.DocFX" / "DocFX.yml")
                .SetPackageFolder(DefinitionRepositoryPath / packageId));
        });
}
