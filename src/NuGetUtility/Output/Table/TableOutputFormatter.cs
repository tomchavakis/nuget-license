using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output.Table
{
    public class TableOutputFormatter : IOutputFormatter
    {
        public async Task Write(Stream stream, IEnumerable<LicenseValidationError> errors)
        {
            await TablePrinterExtensions.Create(stream, "Context", "Package", "Version", "LicenseError")
                .FromValues(errors,
                    error =>
                    {
                        return new object[] { error.Context, error.PackageId, error.PackageVersion, error.Message };
                    })
                .Print();
        }

        public async Task Write(Stream stream, IEnumerable<ValidatedLicense> validated)
        {
            await TablePrinterExtensions
                .Create(stream, "Package", "Version", "License Information Origin", "License Expression")
                .FromValues(
                    validated,
                    license =>
                    {
                        return new object[]
                        {
                            license.PackageId, license.PackageVersion, license.LicenseInformationOrigin, license.License
                        };
                    })
                .Print();
        }
    }
}
