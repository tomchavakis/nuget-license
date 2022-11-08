using NuGet.Protocol.Core.Types;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    internal class WrappedSourceRepositoryProvider : IWrappedSourceRepositoryProvider, IDisposable
    {
        private readonly IDisposableSourceRepository[] _repositories;

        public WrappedSourceRepositoryProvider(ISourceRepositoryProvider provider)
        {
            _repositories = provider.GetRepositories().Where(r => r.PackageSource.IsEnabled).Select(r => new CachingDisposableSourceRepository(r)).ToArray();
        }

        public void Dispose()
        {
            foreach (IDisposableSourceRepository repository in _repositories)
            {
                repository.Dispose();
            }
        }

        public ISourceRepository[] GetRepositories()
        {
            return _repositories;
        }
    }
}
