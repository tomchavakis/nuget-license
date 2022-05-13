using Microsoft.Build.Evaluation;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    internal class ProjectWrapper : IProject
    {
        private const string ProjectAssetsFile = "ProjectAssetsFile";
        private const string PackageReferenceTypeTag = "PackageReference";
        private const string RestoreStyleTag = "RestoreProjectStyle";
        private const string NugetStyleTag = "NuGetProjectStyle";
        private const string PackageReferenceValue = "PackageReference";
        private const string PackagesConfigFileName = "packages.config";

        private readonly Project _project;

        public ProjectWrapper(Project project)
        {
            _project = project;
        }

        public string GetAssetsPath()
        {
            return _project.GetPropertyValue(ProjectAssetsFile);
        }

        public bool HasNugetPackagesReferenced()
        {
            return _project.GetItems(PackageReferenceTypeTag).Count != 0;
        }

        public bool IsNotPackageReferenceProject()
        {
            return StringIsSetAndUnequalTo(_project.GetPropertyValue(RestoreStyleTag), PackageReferenceValue) ||
                   StringIsSetAndUnequalTo(_project.GetPropertyValue(NugetStyleTag), PackageReferenceValue) ||
                   _project.AllEvaluatedItems.Any(projectItem =>
                       projectItem.EvaluatedInclude == PackagesConfigFileName);
        }

        private static bool StringIsSetAndUnequalTo(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            return source != target;
        }
    }
}
