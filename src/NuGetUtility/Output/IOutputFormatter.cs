using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output
{
    public interface IOutputFormatter
    {
        Task Write(Stream stream, IEnumerable<LicenseValidationError> errors);
        Task Write(Stream stream, IEnumerable<ValidatedLicense> validated);
    }
}
