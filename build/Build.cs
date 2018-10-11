// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;

partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    const string c_addonRepoOwner = "nuke-build";
    const string c_addonRepoName = "docfx";
    const string c_addonName = "DocFX";
    const string c_toolNamespace = "Nuke.DocFX";

    public Build()
    {
        InitGeneration();
        InitGitFlow();
    }

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;
    [Solution] readonly Solution Solution;

    [Parameter] readonly string Configuration = IsLocalBuild ? "Debug" : "Release";
    [Parameter("Api key to push packages to NuGet.org.")] readonly string NuGetApiKey;
    [Parameter("Api key to access the GitHub.")] readonly string GitHubApiKey;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    ChangeLog ChangeLogContent => ReadChangelog(ChangelogFile);
    Nuke.Common.ProjectModel.Project AddonProject => Solution.GetProject(c_toolNamespace).NotNull();
    AbsolutePath PackageDirectory => TemporaryDirectory / "packages";

    Target Clean => _ => _
        .Executes(() =>
        {
            DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
            EnsureCleanDirectory(OutputDirectory);
        });

    Target CompileAddon => _ => _
        .DependsOn(GenerateAddon, Clean)
        .Executes(() =>
        {
            DotNetRestore(x => x
                .SetProjectFile(AddonProject));
            DotNetBuild(x => x
                .SetProjectFile(AddonProject)
                .EnableNoRestore()
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
                .SetFileVersion(GitVersion.GetNormalizedFileVersion())
                .SetInformationalVersion(GitVersion.InformationalVersion));
        });

    Target Pack => _ => _
        .DependsOn(CompileAddon)
        .Executes(() =>
        {
            var releaseNotes = ExtractChangelogSectionNotes(ChangelogFile)
                .Select(x => x.Replace("- ", "\u2022 ").Replace("`", string.Empty).Replace(",", "%2C"))
                .Concat(string.Empty)
                .Concat($"Full changelog at {GitRepository.GetGitHubBrowseUrl(ChangelogFile)}")
                .JoinNewLine();

            DotNetPack(s => s
                .SetProject(AddonProject)
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .EnableIncludeSymbols()
                .SetOutputDirectory(OutputDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetPackageReleaseNotes(releaseNotes));
        });

    Target Push => _ => _
        .DependsOn(Pack)
        .Requires(() => NuGetApiKey)
        .Requires(() => GitHasCleanWorkingCopy())
        .Requires(() => Configuration.EqualsOrdinalIgnoreCase("release"))
        .Requires(() => IsReleaseBranch || IsMasterBranch)
        .Executes(() =>
        {
            GlobFiles(OutputDirectory, "*.nupkg")
                .Where(x => !x.EndsWith(".symbols.nupkg")).NotEmpty()
                .ForEach(x => DotNetNuGetPush(s => s
                    .SetTargetPath(x)
                    .SetSource("https://api.nuget.org/v3/index.json")
                    .SetSymbolSource("https://nuget.smbsrc.net/")
                    .SetApiKey(NuGetApiKey)));
        });

    partial void InitGeneration();
    partial void InitGitFlow();
}
