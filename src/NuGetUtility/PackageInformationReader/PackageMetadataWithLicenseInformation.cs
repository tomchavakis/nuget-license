using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Licenses;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGetUtility.PackageInformationReader
{
    internal class PackageMetadataWithLicenseInformation : IPackageSearchMetadata
    {
        private readonly IPackageSearchMetadata _baseMetadata;

        public PackageMetadataWithLicenseInformation(IPackageSearchMetadata baseMetadata, string licenseType)
        {
            _baseMetadata = baseMetadata;
            LicenseMetadata = new LicenseMetadata(LicenseType.Expression, licenseType,
                NuGetLicenseExpression.Parse(licenseType), new string[] { }, LicenseMetadata.EmptyVersion);
        }

        public Task<PackageDeprecationMetadata> GetDeprecationMetadataAsync()
        {
            return _baseMetadata.GetDeprecationMetadataAsync();
        }

        public Task<IEnumerable<VersionInfo>> GetVersionsAsync()
        {
            return _baseMetadata.GetVersionsAsync();
        }

        public string Authors => _baseMetadata.Authors;

        public IEnumerable<PackageDependencyGroup> DependencySets => _baseMetadata.DependencySets;

        public string Description => _baseMetadata.Description;

        public long? DownloadCount => _baseMetadata.DownloadCount;

        public Uri IconUrl => _baseMetadata.IconUrl;

        public PackageIdentity Identity => _baseMetadata.Identity;

        public Uri LicenseUrl => LicenseMetadata.LicenseUrl;

        public Uri ProjectUrl => _baseMetadata.ProjectUrl;

        public Uri ReadmeUrl => _baseMetadata.ReadmeUrl;

        public Uri ReportAbuseUrl => _baseMetadata.ReportAbuseUrl;

        public Uri PackageDetailsUrl => _baseMetadata.PackageDetailsUrl;

        public DateTimeOffset? Published => _baseMetadata.Published;

        public string Owners => _baseMetadata.Owners;

        public bool RequireLicenseAcceptance => _baseMetadata.RequireLicenseAcceptance;

        public string Summary => _baseMetadata.Summary;

        public string Tags => _baseMetadata.Tags;

        public string Title => _baseMetadata.Title;

        public bool IsListed => _baseMetadata.IsListed;

        public bool PrefixReserved => _baseMetadata.PrefixReserved;

        public LicenseMetadata LicenseMetadata { get; }

        public IEnumerable<PackageVulnerabilityMetadata> Vulnerabilities => _baseMetadata.Vulnerabilities;
    }
}
