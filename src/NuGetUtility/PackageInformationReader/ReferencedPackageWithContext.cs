using NuGetUtility.Wrapper.NuGetWrapper.Packaging;

namespace NuGetUtility.PackageInformationReader
{
    public record ReferencedPackageWithContext(string Context, IPackageMetadata PackageInfo);
}
