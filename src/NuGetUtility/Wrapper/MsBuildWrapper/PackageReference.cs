using NuGet.Versioning;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public record PackageReference(string PackageName, NuGetVersion? Version);
}
