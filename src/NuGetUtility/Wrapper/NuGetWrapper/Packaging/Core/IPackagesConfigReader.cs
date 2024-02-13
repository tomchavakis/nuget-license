// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core
{
    public interface IPackagesConfigReader
    {
        IEnumerable<PackageIdentity> GetPackages(IProject project);
    }
}
