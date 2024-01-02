using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core
{
    public class FailingPackagesConfigReader : IPackagesConfigReader
    {
        public IEnumerable<PackageIdentity> GetPackages(IProject project)
        {
            throw new PackagesConfigReaderException($"Invalid project structure detected. Currently packages.config projects are only supported on Windows (Project: {project.FullPath})");
        }
    }
}
