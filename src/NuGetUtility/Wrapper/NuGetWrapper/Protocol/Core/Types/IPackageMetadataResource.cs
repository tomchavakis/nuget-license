using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface IPackageMetadataResource
    {
        Task<IPackageSearchMetadata?>
            TryGetMetadataAsync(PackageIdentity identity, CancellationToken cancellationToken);
    }
}
