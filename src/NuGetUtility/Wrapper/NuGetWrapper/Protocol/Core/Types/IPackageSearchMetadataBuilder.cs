using NuGet.Protocol.Core.Types;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface IPackageSearchMetadataBuilder
    {
        IPackageSearchMetadata Build();
    }
}
