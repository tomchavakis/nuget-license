using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging
{
    public interface IPackageMetadata
    {
        PackageIdentity Identity { get; }
        string Title { get; }
        Uri? LicenseUrl { get; }
        string ProjectUrl { get; }
        string Description { get; }
        string Summary { get; }
        LicenseMetadata? LicenseMetadata { get; }
    }
}
