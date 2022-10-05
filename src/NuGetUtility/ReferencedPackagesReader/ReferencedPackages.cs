using NuGet.Protocol.Core.Types;
using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.ReferencedPackagesReader
{
    public class ReferencedPackages : IReferencedPackages
    {
        public List<IPackageSearchMetadata> Packages { get; } = new List<IPackageSearchMetadata>();
        public List<PackageIdentity> Ignored { get; } = new List<PackageIdentity>();
        public IEnumerable<IPackageSearchMetadata> PackagesToValidate => Packages;
        public IEnumerable<PackageIdentity> IgnoredPackages => Ignored;
    }
}
