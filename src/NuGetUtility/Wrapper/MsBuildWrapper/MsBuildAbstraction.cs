using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    internal class MsBuildAbstraction : IMsBuildAbstraction
    {
        private const string CollectPackageReferences = "CollectPackageReferences";
        private const string PackageReferenceTypeTag = "PackageReference";
        private const string RestoreStyleTag = "RestoreProjectStyle";
        private const string NugetStyleTag = "NuGetProjectStyle";
        private const string AssetsFilePathTag = "ProjectAssetsFile";

        public MsBuildAbstraction()
        {
            MSBuildLocator.RegisterDefaults();
        }

        public IEnumerable<PackageReference> GetPackageReferencesFromProjectForFramework(string projectPath,
            string framework)
        {
            var globalProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "TargetFramework", framework }
            };
            var newProject = new ProjectInstance(projectPath, globalProperties, null);
            newProject.Build(new[] { CollectPackageReferences }, new List<ILogger>(), out var targetOutputs);

            return targetOutputs.First(e => e.Key.Equals(CollectPackageReferences)).Value.Items.Select(p =>
                new PackageReference(p.ItemSpec,
                    p.GetMetadata("version") == null ? null : new WrappedNuGetVersion(p.GetMetadata("version"))));
        }

        public IProject GetProject(string projectPath)
        {
            var rootElement = TryGetProjectRootElement(projectPath);

            var project = new Project(rootElement);

            if (!IsPackageReferenceProject(project))
            {
                throw new MsBuildAbstractionException(
                    $"Invalid project structure detected. Currently only PackageReference projects are supported (Project: {project.FullPath})");
            }

            return new ProjectWrapper(project);
        }

        private static ProjectRootElement TryGetProjectRootElement(string projectPath)
        {
            try
            {
                return ProjectRootElement.Open(projectPath, ProjectCollection.GlobalProjectCollection)!;
            }
            catch (InvalidProjectFileException e)
            {
                throw new MsBuildAbstractionException($"Failed to open project: {projectPath}", e);
            }
        }

        private static bool IsPackageReferenceProject(Project project)
        {
            return (project.GetPropertyValue(RestoreStyleTag) == "PackageReference") ||
                   (project.GetItems(PackageReferenceTypeTag).Count != 0) ||
                   (project.GetPropertyValue(NugetStyleTag) == "PackageReference") ||
                   (project.GetPropertyValue(AssetsFilePathTag) != "");
        }
    }
}
