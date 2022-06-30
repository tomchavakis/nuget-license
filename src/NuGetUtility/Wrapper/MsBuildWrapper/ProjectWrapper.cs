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
            return _project.GetPropertyValue(ProjectAssetsFile);
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
    }
}
