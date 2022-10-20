using NuGet.Protocol.Core.Types;

namespace NuGetUtility.PackageInformationReader
{
    public record ReferencedPackageWithContext(string Context, IPackageSearchMetadata PackageInfo);
}
