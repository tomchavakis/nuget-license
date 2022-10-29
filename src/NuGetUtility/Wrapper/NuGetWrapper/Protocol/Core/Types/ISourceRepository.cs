namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface ISourceRepository
    {
        Task<IPackageMetadataResource?> GetPackageMetadataResourceAsync();
    }
}
