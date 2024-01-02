using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core
{
    public interface IPackagesConfigReader
    {
        IEnumerable<PackageIdentity> GetPackages(IProject project);
    }
}
