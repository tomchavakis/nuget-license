using System.Management;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using NuGetUtility.Extensions;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public class MsBuildAbstraction : IMsBuildAbstraction
    {
        private const string CollectPackageReferences = "CollectPackageReferences";
        private readonly Dictionary<string, string> _project_props = new();

        public MsBuildAbstraction()
        {
            RegisterMsBuildLocatorIfNeeded();

            // to support VC-projects we need to workaround : https://github.com/3F/MvsSln/issues/1
            // adding 'VCTargetsPath' to Project::GlobalProperties seem to be enough

            if (GetBestVCTargetsPath() is string path)
                _project_props.Add("VCTargetsPath", $"{path}\\");
        }

        public IEnumerable<PackageReference> GetPackageReferencesFromProjectForFramework(IProject project,
            string framework)
        {
            var globalProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "TargetFramework", framework }
            };
            var newProject = new ProjectInstance(project.FullPath, globalProperties, null);
            newProject.Build(new[] { CollectPackageReferences }, new List<ILogger>(), out IDictionary<string, TargetResult>? targetOutputs);

            return targetOutputs.First(e => e.Key.Equals(CollectPackageReferences))
                .Value.Items.Select(p =>
                    new PackageReference(p.ItemSpec,
                        string.IsNullOrEmpty(p.GetMetadata("version"))
                            ? null
                            : new WrappedNuGetVersion(p.GetMetadata("version"))));
        }

        public IProject GetProject(string projectPath)
        {
            ProjectRootElement rootElement = TryGetProjectRootElement(projectPath);

            var project = new Project(rootElement, _project_props, null);

            return new ProjectWrapper(project);
        }

        public IEnumerable<string> GetProjectsFromSolution(string inputPath)
        {
            string absolutePath = Path.GetFullPath(inputPath, Environment.CurrentDirectory);
            var sln = SolutionFile.Parse(absolutePath);
            return sln.ProjectsInOrder.Select(p => p.AbsolutePath);
        }

        private static string? GetBestVCTargetsPath()
        {
            var cpp_props = new List<FileInfo>();

            foreach (string path in GetVisualStudioInstallPaths())
                cpp_props.AddRange(new DirectoryInfo(path).GetFiles("Microsoft.Cpp.Default.props", SearchOption.AllDirectories));

            // if multiple, assume most recent 'LastWriteTime' property is 'best'
            return cpp_props.OrderBy(f => f.LastWriteTime).LastOrDefault()?.DirectoryName;
        }

        private static IEnumerable<string> GetVisualStudioInstallPaths()
        {
            // https://learn.microsoft.com/en-us/visualstudio/install/tools-for-managing-visual-studio-instances?view=vs-2022#using-windows-management-instrumentation-wmi

            var result = new List<string>();

            if (OperatingSystem.IsWindows())
            {
                var mmc = new ManagementClass("root/cimv2/vs:MSFT_VSInstance");

                foreach (ManagementBaseObject? vs_instance in mmc.GetInstances())
                    if (vs_instance["InstallLocation"] is string install_path)
                        result.Add(install_path);
            }

            return result;
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
    }
}
