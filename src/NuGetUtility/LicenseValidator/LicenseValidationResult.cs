using NuGet.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record LicenseValidationResult(string PackageId,
        NuGetVersion PackageVersion,
        string? PackageProjectUrl,
        string? License,
        LicenseInformationOrigin LicenseInformationOrigin,
        List<ValidationError>? ValidationErrors = null)
    {
        public List<ValidationError> ValidationErrors { get; } = ValidationErrors ?? new List<ValidationError>();

        public string? License { get; set; } = License;
        public LicenseInformationOrigin LicenseInformationOrigin { get; set; } = LicenseInformationOrigin;
    }
}
