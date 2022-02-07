using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public interface IPackageSearchMetadataBuilderFactory
    {
        IPackageSearchMetadataBuilder FromIdentity(PackageIdentity packageIdentity);
    }
}
