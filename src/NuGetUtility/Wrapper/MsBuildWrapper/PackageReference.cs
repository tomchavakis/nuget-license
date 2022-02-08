using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public record PackageReference(string PackageName, INuGetVersion? Version);
}
