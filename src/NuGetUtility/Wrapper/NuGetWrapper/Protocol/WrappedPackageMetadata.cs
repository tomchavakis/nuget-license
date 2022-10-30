using NuGet.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;
using System.ComponentModel;
using IWrappedPackageMetadata = NuGetUtility.Wrapper.NuGetWrapper.Packaging.IPackageMetadata;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol
{
    internal class WrappedPackageMetadata : IWrappedPackageMetadata
    {
        private ManifestMetadata _metadata;

        public WrappedPackageMetadata(ManifestMetadata metadata)
        {
            Identity = new PackageIdentity(metadata.Id, new WrappedNuGetVersion(metadata.Version));
            LicenseMetadata = (metadata.LicenseMetadata == null) ? null : new Packaging.LicenseMetadata(Convert(metadata.LicenseMetadata.Type), metadata.LicenseMetadata.LicenseExpression?.ToString() ?? string.Empty);
            _metadata = metadata;
        }

        private Packaging.LicenseType Convert(LicenseType type)
        {
            return type switch
            {
                LicenseType.Expression => Packaging.LicenseType.Expression,
                LicenseType.File => Packaging.LicenseType.File,
                _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(LicenseType)),
            };
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
