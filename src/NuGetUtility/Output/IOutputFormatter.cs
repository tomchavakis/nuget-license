using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output
{
    public interface IOutputFormatter
    {
        Task Write(Stream stream, IList<LicenseValidationResult> results);
    }
}
