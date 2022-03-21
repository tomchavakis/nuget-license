using NuGet.Versioning;

namespace NuGetUtility.PackageInformationReader
{
    public record struct CustomPackageInformation(string Id, NuGetVersion Version, string License);
}
