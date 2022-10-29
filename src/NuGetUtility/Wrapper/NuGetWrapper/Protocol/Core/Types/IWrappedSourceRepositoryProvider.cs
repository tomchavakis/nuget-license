namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface IWrappedSourceRepositoryProvider
    {
        ISourceRepository[] GetRemoteRepositories();
        ISourceRepository[] GetLocalRepositories();
    }
}
