// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Diagnostics.CodeAnalysis;
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

            if (TryGetInstalledPackagesFromAssetsFile(includeTransitive, project, out IEnumerable<PackageIdentity>? dependencies))
            {
                return dependencies;
            }

            if (project.HasPackagesConfigFile())
            {
                return _packagesConfigReader.GetPackages(project);
            }

            return Array.Empty<PackageIdentity>();
        }

        private bool TryGetInstalledPackagesFromAssetsFile(bool includeTransitive,
            IProject project,
            [NotNullWhen(true)] out IEnumerable<PackageIdentity>? installedPackages)
        {
            installedPackages = null;
            if (!TryLoadAssetsFile(project, out ILockFile? assetsFile))
            {
                return false;
            }

            var referencedLibraries = new HashSet<ILockFileLibrary>();

            foreach (ILockFileTarget target in assetsFile.Targets!)
            {
                IEnumerable<ILockFileLibrary> referencedLibrariesForTarget =
                    GetReferencedLibrariesForTarget(project, includeTransitive, assetsFile, target);
                referencedLibraries.AddRange(referencedLibrariesForTarget);
            }

            installedPackages = referencedLibraries.Select(r => new PackageIdentity(r.Name, r.Version));
            return true;
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
                IEnumerable<string> directlyReferencedPackages = _msBuild.GetPackageReferencesFromProjectForFramework(project,
                    targetFrameworkInformation.FrameworkName.ToString()!);

                referencedLibrariesForTarget =
                    referencedLibrariesForTarget.Where(l => IsDirectlyReferenced(l, directlyReferencedPackages));
            }

            return referencedLibrariesForTarget;
        }

        private bool IsDirectlyReferenced(ILockFileLibrary library,
            IEnumerable<string> directlyReferencedPackages)
        {
            return directlyReferencedPackages.Any(p =>
                library.Name.Equals(p, StringComparison.OrdinalIgnoreCase));
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

        private bool TryLoadAssetsFile(IProject project, [NotNullWhen(true)] out ILockFile? assetsFile)
        {
            if (!project.TryGetAssetsPath(out string assetsPath))
            {
                assetsFile = null;
                return false;
            }
            assetsFile = _lockFileFactory.GetFromFile(assetsPath);

            if (!assetsFile.PackageSpec.IsValid() || !(assetsFile.Targets?.Any() ?? false))
            {
                throw new ReferencedPackageReaderException(
                    $"Failed to validate project assets for project {project.FullPath}");
            }

            return true;
        }
    }
}
