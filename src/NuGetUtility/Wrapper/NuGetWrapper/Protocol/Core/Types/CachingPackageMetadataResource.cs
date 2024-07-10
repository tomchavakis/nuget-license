// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGet.Common;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

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
                IPackageSearchMetadata result = await _metadataResource.GetMetadataAsync(new NuGet.Packaging.Core.PackageIdentity(identity.Id, new NuGetVersion(identity.Version.ToString()!)),
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

        private sealed class WrappedPackageSearchMetadata : IPackageMetadata
        {
            private readonly IPackageSearchMetadata _searchMetadata;

            public WrappedPackageSearchMetadata(IPackageSearchMetadata searchMetadata)
            {
                Identity = new PackageIdentity(searchMetadata.Identity.Id, new WrappedNuGetVersion(searchMetadata.Identity.Version));
                LicenseMetadata = searchMetadata.LicenseMetadata;
                _searchMetadata = searchMetadata;
            }

            public PackageIdentity Identity { get; }

            public string? Title => _searchMetadata.Title;

            public Uri? LicenseUrl => _searchMetadata.LicenseUrl;

            public string? ProjectUrl => _searchMetadata.ProjectUrl?.ToString();

            public string? Description => _searchMetadata.Description;

            public string? Summary => _searchMetadata.Summary;

            public string? Copyright => null;

            public string? Authors => _searchMetadata.Authors;

            public LicenseMetadata? LicenseMetadata { get; }
        }
    }
}
