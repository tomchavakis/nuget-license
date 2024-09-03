// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public class MsBuildAbstraction : IMsBuildAbstraction
    {
        private const string CollectPackageReferences = "CollectPackageReferences";
        private ProjectCollection? _projects;

        private ProjectCollection Projects => _projects ??= InitializeProjectCollection();

        public MsBuildAbstraction()
        {
            RegisterMsBuildLocatorIfNeeded();
        }

        public IEnumerable<string> GetPackageReferencesFromProjectForFramework(IProject project,
            string framework)
        {
            var globalProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "TargetFramework", framework }
            };
            var newProject = new ProjectInstance(project.FullPath, globalProperties, null);
            newProject.Build(new[] { CollectPackageReferences }, new List<ILogger>(), out IDictionary<string, TargetResult>? targetOutputs);

            return targetOutputs.First(e => e.Key.Equals(CollectPackageReferences))
                .Value.Items.Select(p => p.ItemSpec);
        }

        public IProject GetProject(string projectPath)
        {
#if !NETFRAMEWORK
            if (projectPath.EndsWith("vcxproj"))
            {
                throw new MsBuildAbstractionException($"Please use the .net Framework version to analyze c++ projects (Project: {projectPath})");
            }
#endif

            Project project = Projects.LoadProject(projectPath);

            return new ProjectWrapper(project);
        }

        public IEnumerable<string> GetProjectsFromSolution(string inputPath)
        {
            string absolutePath = Path.IsPathRooted(inputPath) ? inputPath : Path.Combine(Environment.CurrentDirectory, inputPath);
            var sln = SolutionFile.Parse(absolutePath);
            return sln.ProjectsInOrder.Select(p => p.AbsolutePath);
        }

        private static void RegisterMsBuildLocatorIfNeeded()
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
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

        private static ProjectCollection InitializeProjectCollection()
        {
            ProjectCollection collection = ProjectCollection.GlobalProjectCollection;
            collection.UnloadAllProjects();
            return collection;
        }
    }
}
