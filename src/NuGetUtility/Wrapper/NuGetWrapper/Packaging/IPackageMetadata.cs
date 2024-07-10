// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.Wrapper.NuGetWrapper.Packaging
{
    public interface IPackageMetadata
    {
        PackageIdentity Identity { get; }
        string? Title { get; }
        Uri? LicenseUrl { get; }
        string? ProjectUrl { get; }
        string? Description { get; }
        string? Summary { get; }
        string? Copyright { get; }
        string? Authors { get; }
        LicenseMetadata? LicenseMetadata { get; }
    }
}
