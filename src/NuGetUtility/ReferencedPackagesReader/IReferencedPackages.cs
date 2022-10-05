using NuGet.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.ReferencedPackagesReader
{
    public interface IReferencedPackages
    {
        public IEnumerable<IPackageSearchMetadata> PackagesToValidate { get; }
        public IEnumerable<PackageIdentity> IgnoredPackages { get; }
    }
}
