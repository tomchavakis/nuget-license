using NuGet.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record ValidatedLicense(string PackageId,
        NuGetVersion PackageVersion,
        string License,
        LicenseInformationOrigin LicenseInformationOrigin,
        Uri? ProjectUrl = null);
}
