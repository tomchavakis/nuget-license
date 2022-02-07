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

        public string GetAssetsPath()
        {
            return _project.GetPropertyValue(ProjectAssetsFile);
        }
    }
}
