using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol
{
    public interface IGlobalPackagesFolderUtility
    {
        IPackageMetadata? GetPackage(PackageIdentity identity);
    }
}
