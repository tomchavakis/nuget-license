using NuGet.Common;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;
using System.ComponentModel;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    internal class CachingPackageMetadataResource : IPackageMetadataResource
    {
        private readonly SourceCacheContext _cacheContext;
        private readonly PackageMetadataResource _metadataResource;

        public CachingPackageMetadataResource(PackageMetadataResource metadataResource, SourceCacheContext cacheContext)
        {
            _metadataResource = metadataResource;
            _cacheContext = cacheContext;
        }

        public async Task<IPackageMetadata?> TryGetMetadataAsync(PackageIdentity identity,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _metadataResource.GetMetadataAsync(new NuGet.Packaging.Core.PackageIdentity(identity.Id, new NuGetVersion(identity.Version.ToString())),
                    _cacheContext,
                    new NullLogger(),
                    cancellationToken);
                return new WrappedPackageSearchMetadata(result);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private class WrappedPackageSearchMetadata : IPackageMetadata
        {
            private IPackageSearchMetadata _searchMetadata;

            public WrappedPackageSearchMetadata(IPackageSearchMetadata searchMetadata)
            {
                Identity = new PackageIdentity(searchMetadata.Identity.Id, new WrappedNuGetVersion(searchMetadata.Identity.Version));
                LicenseMetadata = searchMetadata.LicenseMetadata;
                _searchMetadata = searchMetadata;
            }

            private LicenseType Convert(NuGet.Packaging.LicenseType type)
            {
                return type switch
                {
                    NuGet.Packaging.LicenseType.File => LicenseType.File,
                    NuGet.Packaging.LicenseType.Expression => LicenseType.Expression,
                    _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(NuGet.Packaging.LicenseType)),
                };
            }

            public PackageIdentity Identity { get; }

            public string Title => _searchMetadata.Title;

            public Uri? LicenseUrl => _searchMetadata.LicenseUrl;

            public string ProjectUrl => _searchMetadata.ProjectUrl?.ToString() ?? string.Empty;

            public string Description => _searchMetadata.Description;

            public string Summary => _searchMetadata.Summary;

            public LicenseMetadata? LicenseMetadata { get; }
        }
    }
}
