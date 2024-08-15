// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Evaluation;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    internal class ProjectWrapper : IProject
    {
        private const string ProjectAssetsFile = "ProjectAssetsFile";

        private readonly Project _project;

        public ProjectWrapper(Project project)
        {
            _project = project;
        }

        public bool TryGetAssetsPath([NotNullWhen(true)] out string assetsFile)
        {
            assetsFile = _project.GetPropertyValue(ProjectAssetsFile);
            if (string.IsNullOrEmpty(assetsFile))
            {
                return false;
            }

            if (!File.Exists(assetsFile))
            {
                throw new MsBuildAbstractionException(
                    $"Failed to get the project assets file for project {_project.FullPath} ({assetsFile})");
            }

            return true;
        }

        public IEnumerable<string> GetEvaluatedIncludes()
        {
            return _project.AllEvaluatedItems.Select(projectItem => projectItem.EvaluatedInclude);
        }

        public string FullPath => _project.FullPath;
    }
}
