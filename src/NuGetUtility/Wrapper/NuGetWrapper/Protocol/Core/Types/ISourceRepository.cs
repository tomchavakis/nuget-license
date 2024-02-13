// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface ISourceRepository
    {
        Task<IPackageMetadataResource?> GetPackageMetadataResourceAsync();
    }
}
