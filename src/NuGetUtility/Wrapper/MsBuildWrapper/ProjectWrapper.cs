using Microsoft.Build.Evaluation;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    internal class ProjectWrapper : IProject
    {
        private const string PackageReferenceTypeTag = "PackageReference";
        private const string ProjectAssetsFile = "ProjectAssetsFile";
        private const string RestoreStyleTag = "RestoreProjectStyle";
        private const string NugetStyleTag = "NuGetProjectStyle";

        private readonly Project _project;

        public ProjectWrapper(Project project)
        {
            _project = project;
        }

        public string GetAssetsPath()
        {
            var assetsFile = _project.GetPropertyValue(ProjectAssetsFile);
            if (!File.Exists(assetsFile))
            {
                throw new MsBuildAbstractionException(
                    $"Failed to get the project assets file for project {_project.FullPath}");
            }

            return assetsFile;
        }

        public string GetRestoreStyleTag()
        {
            return _project.GetPropertyValue(RestoreStyleTag);
        }

        public string GetNugetStyleTag()
        {
            return _project.GetPropertyValue(NugetStyleTag);
        }

        public int GetPackageReferenceCount()
        {
            return _project.GetItems(PackageReferenceTypeTag).Count;
        }

        public IEnumerable<string> GetEvaluatedIncludes()
        {
            return _project.AllEvaluatedItems.Select(projectItem => projectItem.EvaluatedInclude);
        }

        public string FullPath => _project.FullPath;
    }
}
