using NuGet.Protocol.Core.Types;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    internal class WrappedSourceRepositoryProvider : IWrappedSourceRepositoryProvider, IDisposable
    {
        private readonly IDisposableSourceRepository[] _localRepositories;
        private readonly IDisposableSourceRepository[] _remoteRepositories;

        public WrappedSourceRepositoryProvider(ISourceRepositoryProvider provider)
        {
            var localAndRemoteRepositories = provider.GetRepositories().Where(r => r.PackageSource.IsEnabled).ToLookup(r => r.PackageSource.IsLocal);
            _localRepositories = localAndRemoteRepositories[true].Select(r => new CachingDisposableSourceRepository(r)).ToArray();
            _remoteRepositories = localAndRemoteRepositories[false].Select(r => new CachingDisposableSourceRepository(r)).ToArray();
        }

        public void Dispose()
        {
            foreach (var repository in _localRepositories)
            {
                repository.Dispose();
            }
            foreach (var repository in _remoteRepositories)
            {
                repository.Dispose();
            }
        }

        public ISourceRepository[] GetRemoteRepositories()
        {
            return _remoteRepositories;
        }

        public ISourceRepository[] GetLocalRepositories()
        {
            return _localRepositories;
        }
    }
}
