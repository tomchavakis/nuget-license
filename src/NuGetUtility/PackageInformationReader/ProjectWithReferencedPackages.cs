using NuGetUtility.Wrapper.NuGetWrapper.Packaging.Core;

namespace NuGetUtility.PackageInformationReader
{
    public record ProjectWithReferencedPackages(string Project, IEnumerable<PackageIdentity> ReferencedPackages);
}
