// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/docfx/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.CodeGeneration.Model;

namespace Nuke.DocFX.Generator
{
    public abstract class SpecificationParser : ISpecificationParser
    {
        public abstract void Dispose();
        protected abstract List<string> ParseReferences();
        protected abstract List<Task> ParseTasks(Tool tool);

        protected virtual List<Enumeration> ParseEnumerations()
        {
            return new List<Enumeration>();
        }

        protected virtual List<Property> ParseCommonTaskProperties()
        {
            return new List<Property>();
        }

        protected virtual Dictionary<string, List<Property>> ParseCommonTaskPropertySets()
        {
            return new Dictionary<string, List<Property>>();
        }

        protected virtual List<DataClass> ParseDataClasses(Tool tool)
        {
            return new List<DataClass>();
        }

        protected virtual void PostPopulate(Tool tool)
        {
        }

        public void Populate(Tool tool)
        {
            tool.References = ParseReferences();
            tool.Tasks = ParseTasks(tool);
            tool.CommonTaskProperties = ParseCommonTaskProperties();
            tool.CommonTaskPropertySets = ParseCommonTaskPropertySets();
            tool.DataClasses = ParseDataClasses(tool);
            tool.Enumerations = ParseEnumerations();
            PostPopulate(tool);
        }
    }
}
