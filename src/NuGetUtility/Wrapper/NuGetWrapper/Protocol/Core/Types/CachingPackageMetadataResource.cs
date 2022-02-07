using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

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

        public async Task<IPackageSearchMetadata?> TryGetMetadataAsync(PackageIdentity identity,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _metadataResource.GetMetadataAsync(identity, _cacheContext, new NullLogger(),
                    cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
