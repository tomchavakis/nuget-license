using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.Test.ReferencedPackagesReader.PackageIdentityMock
{
    internal record struct PackageSearchMetadataMock(PackageIdentity Id) : IPackageSearchMetadata
    {
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

        public NuGet.Packaging.Core.PackageIdentity Identity => throw new NotImplementedException();

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

        public LicenseMetadata LicenseMetadata => throw new NotImplementedException();

        public IEnumerable<PackageVulnerabilityMetadata> Vulnerabilities => throw new NotImplementedException();
    }
}
