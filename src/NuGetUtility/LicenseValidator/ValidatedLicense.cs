using NuGet.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record struct ValidatedLicense(string PackageId, NuGetVersion PackageVersion, string License);
}
