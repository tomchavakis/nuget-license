using NuGet.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record struct LicenseValidationError(string Context, string PackageId, NuGetVersion PackageVersion,
        string Message);
}
