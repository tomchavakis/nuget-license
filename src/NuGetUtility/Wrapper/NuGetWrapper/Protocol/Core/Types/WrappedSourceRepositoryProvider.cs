using NuGet.Protocol.Core.Types;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    internal class WrappedSourceRepositoryProvider : IWrappedSourceRepositoryProvider
    {
        private readonly ISourceRepositoryProvider _provider;

        public WrappedSourceRepositoryProvider(ISourceRepositoryProvider provider)
        {
            _provider = provider;
        }

        public IEnumerable<IDisposableSourceRepository> GetRepositories()
        {
            return _provider.GetRepositories().Select(r => new CachingDisposableSourceRepository(r)).ToArray();
        }
    }
}
