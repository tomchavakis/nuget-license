using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output.Table
{
    public class TableOutputFormatter : IOutputFormatter
    {

        public async Task Write(Stream stream, IList<LicenseValidationResult> results)
        {
            bool printPackageProjectUrl = false;
            bool printErrors = false;
            foreach (var license in results)
            {
                printPackageProjectUrl |= license.PackageProjectUrl != null;
                printErrors |= license.ValidationErrors.Any();
            }
            var headings = new List<string>(6) { "Package", "Version", "License Information Origin", "License Expression" };
            var formatters = new List<Func<LicenseValidationResult, object?>>(6)
            {
                license => license.PackageId, license => license.PackageVersion,
                license => license.LicenseInformationOrigin,
                license => license.License
            };
            if (printPackageProjectUrl)
            {
                headings.Add("Package Project Url");
                formatters.Add(license => license.PackageProjectUrl);
            }
            if (printErrors)
            {
                headings.Add("Errors");
                formatters.Add(license => FormatErrors(license.ValidationErrors));
            }

            await TablePrinterExtensions
                .Create(stream, headings)
                .FromValues(
                    results,
                    license =>
                    {
                        return formatters.Select(func => func(license));
                    })
                .Print();
        }
        private string FormatErrors(List<ValidationError> errors)
        {
            return string.Join("\n", errors.Select(e => $"Error: {e.Error}, Context:{e.Context}"));
        }
    }
}
