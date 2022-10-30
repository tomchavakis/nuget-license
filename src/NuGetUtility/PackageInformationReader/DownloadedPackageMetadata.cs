using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGetUtility.PackageInformationReader
{
    internal class DownloadedPackageMetadata : IPackageSearchMetadata
    {

        private readonly IPackageMetadata _baseMetadata;

        public DownloadedPackageMetadata(IPackageMetadata baseMetadata)
        {
            _baseMetadata = baseMetadata;
            Identity = new PackageIdentity(baseMetadata.Id, baseMetadata.Version);
        }
        public string Authors => throw new NotImplementedException();

        public IEnumerable<PackageDependencyGroup> DependencySets => throw new NotImplementedException();

        public string Description => _baseMetadata.Description;

        public long? DownloadCount => throw new NotImplementedException();

        public Uri IconUrl => _baseMetadata.IconUrl;

        public PackageIdentity Identity { get; }

        public Uri LicenseUrl => _baseMetadata.LicenseUrl;

        public Uri ProjectUrl => _baseMetadata.ProjectUrl;

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

        public LicenseMetadata LicenseMetadata => throw new NotImplementedException();

        public IEnumerable<PackageVulnerabilityMetadata> Vulnerabilities => throw new NotImplementedException();

        public Task<PackageDeprecationMetadata> GetDeprecationMetadataAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VersionInfo>> GetVersionsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
