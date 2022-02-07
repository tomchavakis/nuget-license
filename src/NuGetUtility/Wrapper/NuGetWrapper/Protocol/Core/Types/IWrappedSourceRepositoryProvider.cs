namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface IWrappedSourceRepositoryProvider
    {
        IEnumerable<IDisposableSourceRepository> GetRepositories();
    }
}
