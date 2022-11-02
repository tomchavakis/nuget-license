using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.PackageInformationReader
{
    public record struct CustomPackageInformation(string Id, INuGetVersion Version, string License);
}
