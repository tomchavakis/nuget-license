// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output
{
    public interface IOutputFormatter
    {
        Task Write(Stream stream, IList<LicenseValidationResult> results);
    }
}
