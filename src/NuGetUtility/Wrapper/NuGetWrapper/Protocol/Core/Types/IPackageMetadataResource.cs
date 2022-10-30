using NuGetUtility.Wrapper.NuGetWrapper.Packaging;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface IPackageMetadataResource
    {
        Task<IPackageMetadata?> TryGetMetadataAsync(PackageIdentity identity, CancellationToken cancellationToken);
    }
}
