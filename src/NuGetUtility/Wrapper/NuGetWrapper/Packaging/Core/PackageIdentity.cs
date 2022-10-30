using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core
{
    public record PackageIdentity(string Id, INuGetVersion Version);
}
