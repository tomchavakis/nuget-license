using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record ValidatedLicense(string PackageId,
        INuGetVersion PackageVersion,
        string License,
        LicenseInformationOrigin LicenseInformationOrigin);
}
