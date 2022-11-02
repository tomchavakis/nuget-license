using NuGet.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;
using IWrappedPackageMetadata = NuGetUtility.Wrapper.NuGetWrapper.Packaging.IPackageMetadata;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol
{
    internal class WrappedPackageMetadata : IWrappedPackageMetadata
    {
        private ManifestMetadata _metadata;

        public WrappedPackageMetadata(ManifestMetadata metadata)
        {
            Identity = new PackageIdentity(metadata.Id, new WrappedNuGetVersion(metadata.Version));
            LicenseMetadata = metadata.LicenseMetadata;
            _metadata = metadata;
        }

        public PackageIdentity Identity { get; }

        public string Title => _metadata.Title;

        public Uri? LicenseUrl => _metadata.LicenseUrl;

        public string ProjectUrl => _metadata.ProjectUrl?.ToString() ?? string.Empty;

        public string Description => _metadata.Description;

        public string Summary => _metadata.Summary;

        public Packaging.LicenseMetadata? LicenseMetadata { get; }
    }
}
