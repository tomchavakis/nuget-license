using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output.Table
{
    public class TableOutputFormatter : IOutputFormatter
    {
        private readonly bool _withProjectUrls;

        public TableOutputFormatter(bool withProjectUrls) => _withProjectUrls = withProjectUrls;

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

        public async Task Write(Stream stream, IEnumerable<ValidatedLicense> validated) =>
            await TablePrinterExtensions.Create(stream, GetHeadings())
                .FromValues(validated, GetFields)
                .Print();

        private object[] GetFields(ValidatedLicense license) =>
            _withProjectUrls ?
                new object[] { license.PackageId, license.PackageVersion, license.LicenseInformationOrigin, license.License, license.ProjectUrl! } :
                new object[] { license.PackageId, license.PackageVersion, license.LicenseInformationOrigin, license.License };

        private string[] GetHeadings()
        {
            var tableHeadings = new List<string> { "Package", "Version", "License Information Origin", "License Expression" };

            if (_withProjectUrls)
                tableHeadings.Add("Project Url");

            return tableHeadings.ToArray();
        }
    }
}
