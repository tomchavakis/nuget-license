using NuGet.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.NuGetWrapper.Protocol.Core.Types
{
    public class PackageSearchMetadataBuilderFactory : IPackageSearchMetadataBuilderFactory
    {
        public IPackageSearchMetadataBuilder FromIdentity(PackageIdentity packageIdentity)
        {
            var wrappedNugetVersion = packageIdentity.Version as WrappedNuGetVersion;

            var transformedPackageIdentity =
                new NuGet.Packaging.Core.PackageIdentity(packageIdentity.Name, wrappedNugetVersion!.Unwrap());
            return new WrappedPackageSearchMetadataBuilder(
                PackageSearchMetadataBuilder.FromIdentity(transformedPackageIdentity));
        }
    }
}
