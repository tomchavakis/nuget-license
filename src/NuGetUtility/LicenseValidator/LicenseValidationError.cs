using NuGet.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record LicenseValidationError(string Context,
        string PackageId,
        NuGetVersion PackageVersion,
        string Message);
}
