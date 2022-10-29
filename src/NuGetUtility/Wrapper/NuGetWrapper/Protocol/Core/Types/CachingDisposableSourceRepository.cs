using NuGet.Protocol.Core.Types;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    internal class CachingDisposableSourceRepository : IDisposableSourceRepository
    {
        private readonly SourceCacheContext _cacheContext = new SourceCacheContext();
        private readonly SourceRepository _sourceRepository;
        private IPackageMetadataResource? _packageMetadataResource;

        public CachingDisposableSourceRepository(SourceRepository repo)
        {
            _sourceRepository = repo;
        }

        public void Dispose()
        {
            _packageMetadataResource = null;
            _cacheContext.Dispose();
        }

        public async Task<IPackageMetadataResource?> GetPackageMetadataResourceAsync()
        {
            if (_packageMetadataResource != null)
            {
                return _packageMetadataResource;
            }

            _packageMetadataResource = new CachingPackageMetadataResource(
                await _sourceRepository.GetResourceAsync<PackageMetadataResource>(),
                _cacheContext);
            return _packageMetadataResource;
        }
    }
}
