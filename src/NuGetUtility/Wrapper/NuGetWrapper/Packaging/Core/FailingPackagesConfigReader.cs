// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

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
