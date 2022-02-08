using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Licenses;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGetUtility.Test.Helper.NuGet.Protocol.Core.Types
{
    internal class PackageMetadataWithVersionInfo : IPackageSearchMetadata
    {
        private readonly string _license;
        private readonly string _packageId;
        private readonly NuGetVersion _packageVersion;

        public PackageMetadataWithVersionInfo(string packageId, NuGetVersion packageVersion, string license)
        {
            _packageId = packageId;
            _packageVersion = packageVersion;
            _license = license;
        }

        public Task<PackageDeprecationMetadata> GetDeprecationMetadataAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VersionInfo>> GetVersionsAsync()
        {
            throw new NotImplementedException();
        }

        public string Authors => throw new NotImplementedException();

        public IEnumerable<PackageDependencyGroup> DependencySets => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public long? DownloadCount => throw new NotImplementedException();

        public Uri IconUrl => throw new NotImplementedException();

        public PackageIdentity Identity => new PackageIdentity(_packageId, _packageVersion);

        public Uri LicenseUrl => throw new NotImplementedException();

        public Uri ProjectUrl => throw new NotImplementedException();

        public Uri ReadmeUrl => throw new NotImplementedException();

        public Uri ReportAbuseUrl => throw new NotImplementedException();

        public Uri PackageDetailsUrl => throw new NotImplementedException();

        public DateTimeOffset? Published => throw new NotImplementedException();

        public string Owners => throw new NotImplementedException();

        public bool RequireLicenseAcceptance => throw new NotImplementedException();

        public string Summary => throw new NotImplementedException();

        public string Tags => throw new NotImplementedException();

        public string Title => throw new NotImplementedException();

        public bool IsListed => throw new NotImplementedException();

        public bool PrefixReserved => throw new NotImplementedException();

        public LicenseMetadata LicenseMetadata =>
            new LicenseMetadata(LicenseType.Expression, _license, NuGetLicenseExpression.Parse(_license),
                new string[] { }, LicenseMetadata.EmptyVersion);

        public IEnumerable<PackageVulnerabilityMetadata> Vulnerabilities => throw new NotImplementedException();
    }
}
