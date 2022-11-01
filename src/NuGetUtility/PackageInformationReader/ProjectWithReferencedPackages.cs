using NuGet.Protocol.Core.Types;

namespace NuGetUtility.PackageInformationReader
{
    public record ProjectWithReferencedPackages(string Project, IEnumerable<IPackageSearchMetadata> ReferencedPackages);
}
