using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.PackageInformationReader
{
    internal class PackageMetadata : IPackageMetadata
    {
        public PackageMetadata(PackageIdentity identity)
        {
            Identity = identity;
        }

        public PackageMetadata(PackageIdentity identity, string licenseIdentifier, LicenseType licenseType)
        {
            Identity = identity;
            LicenseMetadata = new LicenseMetadata(licenseType, licenseIdentifier);
        }

        public PackageIdentity Identity { get; }

        public string Title { get; } = string.Empty;

        public Uri? LicenseUrl => null;

        public string ProjectUrl => string.Empty;

        public string Description => string.Empty;

        public string Summary => string.Empty;

        public LicenseMetadata? LicenseMetadata { get; } = null;
    }
}
