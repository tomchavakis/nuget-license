// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

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

        public PackageMetadata(PackageIdentity identity, LicenseType licenseType, CustomPackageInformation customPackageInformation)
        {
            Identity = identity;
            LicenseMetadata = new LicenseMetadata(licenseType, customPackageInformation.License);
            CustomPackageInformation = customPackageInformation;
        }

        private CustomPackageInformation? CustomPackageInformation { get; }

        public PackageIdentity Identity { get; }

        public string? Title => CustomPackageInformation?.Title;

        public Uri? LicenseUrl => null;

        public string? ProjectUrl => CustomPackageInformation?.ProjectUrl;

        public string? Description => CustomPackageInformation?.Description;

        public string? Summary => CustomPackageInformation?.Summary;

        public string? Copyright => CustomPackageInformation?.Copyright;

        public string? Authors => CustomPackageInformation?.Authors;

        public LicenseMetadata? LicenseMetadata { get; } = null;
    }
}
