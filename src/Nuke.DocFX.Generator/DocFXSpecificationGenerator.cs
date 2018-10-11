// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Linq;

namespace Nuke.DocFX.Generator
{
    public class DocFXSpecificationGenerator : SpecificationGenerator
    {
        public static void WriteSpecifications(DocFXSpecificationGeneratorSettings settings)
        {
            new DocFXSpecificationGenerator(settings).GenerateSpecifications();
        }

        public static void WriteSpecifications(Func<DocFXSpecificationGeneratorSettings, DocFXSpecificationGeneratorSettings> settings)
        {
            WriteSpecifications(settings(new DocFXSpecificationGeneratorSettings()));
        }

        private readonly DocFXSpecificationGeneratorSettings _settings;

        public DocFXSpecificationGenerator(DocFXSpecificationGeneratorSettings settings)
            : base(settings.OutputFolder, settings.OverwriteFile)
        {
            _settings = settings;
        }

        protected override string ToolName => "DocFX";

        protected override string Help =>
            "DocFX is an API documentation generator for .NET, and currently it supports C# and VB. It generates API reference documentation from triple-slash comments in your source code. It also allows you to use Markdown files to create additional topics such as tutorials and how-tos, and to customize the generated reference documentation. DocFX builds a static HTML website from your source code and Markdown files, which can be easily hosted on any web servers (for example, <em>github.io</em>). Also, DocFX provides you the flexibility to customize the layout and style of your website through templates. If you are interested in creating your own website with your own styles, you can follow <a href=\"http://dotnet.github.io/docfx/tutorial/howto_create_custom_template.html\">how to create custom template</a> to create custom templates.";

        protected override string PackageId => "docfx.console";
        protected override string PackageExecutable => "docfx.exe";
        protected override string OfficialUrl => "https://dotnet.github.io/docfx/";

        protected override ISpecificationParser CreateSpecificationParser()
        {
            return DocFXSpecificationParser.FromPackages(_settings.PackageFolder, _settings.GitReference);
        }
    }
}
