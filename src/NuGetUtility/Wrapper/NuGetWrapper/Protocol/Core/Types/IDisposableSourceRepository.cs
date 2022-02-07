namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface IDisposableSourceRepository : IDisposable
    {
        Task<IPackageMetadataResource?> GetPackageMetadataResourceAsync();
    }
}
