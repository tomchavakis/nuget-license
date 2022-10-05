using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record LicenseValidationError(string Context,
        string PackageId,
        INuGetVersion PackageVersion,
        string Message);
}
