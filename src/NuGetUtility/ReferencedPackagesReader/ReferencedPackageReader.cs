using NuGetUtility.Extensions;
using NuGetUtility.Wrapper.MsBuildWrapper;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.ProjectModel;

namespace NuGetUtility.ReferencedPackagesReader
{
    public class ReferencedPackageReader
    {
        private const string ProjectReferenceIdentifier = "project";
        private readonly ILockFileFactory _lockFileFactory;
        private readonly IPackagesConfigReader _packagesConfigReader;
        private readonly IMsBuildAbstraction _msBuild;

        public ReferencedPackageReader(IMsBuildAbstraction msBuild,
            ILockFileFactory lockFileFactory,
            IPackagesConfigReader packagesConfigReader)
        {
            _msBuild = msBuild;
            _lockFileFactory = lockFileFactory;
            _packagesConfigReader = packagesConfigReader;
        }

        public IEnumerable<PackageIdentity> GetInstalledPackages(string projectPath, bool includeTransitive)
        {
            IProject project = _msBuild.GetProject(projectPath);

            if (!project.HasNugetPackagesReferenced() && !includeTransitive)
            {
                return Enumerable.Empty<PackageIdentity>();
            }

            if (project.IsPackageReferenceProject())
                return GetInstalledPackagesFromAssetsFile(includeTransitive, project);

            return _packagesConfigReader.GetPackages(project);
        }

        private IEnumerable<PackageIdentity> GetInstalledPackagesFromAssetsFile(bool includeTransitive,
            IProject project)
        {
            ILockFile assetsFile = LoadAssetsFile(project);

            var referencedLibraries = new HashSet<ILockFileLibrary>();

            foreach (ILockFileTarget target in assetsFile.Targets!)
            {
                IEnumerable<ILockFileLibrary> referencedLibrariesForTarget =
                    GetReferencedLibrariesForTarget(project, includeTransitive, assetsFile, target);
                referencedLibraries.AddRange(referencedLibrariesForTarget);
            }

            return referencedLibraries.Select(r => new PackageIdentity(r.Name, r.Version));
        }

        private IEnumerable<ILockFileLibrary> GetReferencedLibrariesForTarget(IProject project,
            bool includeTransitive,
            ILockFile assetsFile,
            ILockFileTarget target)
        {
            IEnumerable<ILockFileLibrary> referencedLibrariesForTarget = assetsFile.Libraries.Where(l => l.Type != ProjectReferenceIdentifier);

            if (!includeTransitive)
            {
                ITargetFrameworkInformation targetFrameworkInformation = GetTargetFrameworkInformation(target, assetsFile);
                IEnumerable<PackageReference> directlyReferencedPackages = _msBuild.GetPackageReferencesFromProjectForFramework(project,
                    targetFrameworkInformation.FrameworkName.ToString()!);

                referencedLibrariesForTarget =
                    referencedLibrariesForTarget.Where(l => IsDirectlyReferenced(l, directlyReferencedPackages));
            }

            return referencedLibrariesForTarget;
        }

        private bool IsDirectlyReferenced(ILockFileLibrary library,
            IEnumerable<PackageReference> directlyReferencedPackages)
        {
            return directlyReferencedPackages.Any(p =>
                library.Name.Equals(p.PackageName, StringComparison.OrdinalIgnoreCase) && ((p.Version == null) ||
                    library.Version.Equals(p.Version)));
        }

        private static ITargetFrameworkInformation GetTargetFrameworkInformation(ILockFileTarget target,
            ILockFile assetsFile)
        {
            try
            {
                return assetsFile.PackageSpec.TargetFrameworks.First(
                    t => t.FrameworkName.Equals(target.TargetFramework));
            }
            catch (Exception e)
            {
                throw new ReferencedPackageReaderException(
                    $"Failed to identify the target framework information for {target}",
                    e);
            }
        }

        private ILockFile LoadAssetsFile(IProject project)
        {
            string assetsPath = project.GetAssetsPath();
            ILockFile assetsFile = _lockFileFactory.GetFromFile(assetsPath);

            if (!assetsFile.PackageSpec.IsValid() || !(assetsFile.Targets?.Any() ?? false))
            {
                throw new ReferencedPackageReaderException(
                    $"Failed to validate project assets for project {project.FullPath}");
            }

            return assetsFile;
        }
    }
}
