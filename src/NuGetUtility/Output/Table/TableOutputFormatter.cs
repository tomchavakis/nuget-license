using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output.Table
{
    public class TableOutputFormatter : IOutputFormatter
    {
        private readonly bool _omitValidLicensesIfErrorsExist;
        public TableOutputFormatter(bool omitValidLicensesIfErrorsExist = false)
        {
            _omitValidLicensesIfErrorsExist = omitValidLicensesIfErrorsExist;
        }

        public async Task Write(Stream stream, IList<LicenseValidationResult> results)
        {
            bool printPackageProjectUrl = false;
            bool hasPackagesWithErrors = false;
            foreach (var license in results)
            {
                printPackageProjectUrl |= license.PackageProjectUrl != null;
                hasPackagesWithErrors |= license.ValidationErrors.Any();
            }
            var headings = new List<string>(6) { "Package", "Version", "License Information Origin", "License Expression" };
            var formatters = new List<Func<LicenseValidationResult, string?>>(6)
            {
                license => license.PackageId, license => license.PackageVersion.ToString(),
                license => license.LicenseInformationOrigin.ToString(),
                license => license.License
            };
            if (printPackageProjectUrl)
            {
                headings.Add("Package Project Url");
                formatters.Add(license => license.PackageProjectUrl);
            }
            if (hasPackagesWithErrors)
            {
                headings.Add("Errors");
                formatters.Add(license => FormatErrors(license.ValidationErrors));

                if (_omitValidLicensesIfErrorsExist)
                {
                    results = results.Where(r => r.ValidationErrors.Any()).ToList();
                }
            }

            await TablePrinterExtensions
                .Create(stream, headings)
                .FromValues(
                    results,
                    license => formatters.Select(func => func(license)))
                .Print();
        }
        private string FormatErrors(List<ValidationError> errors)
        {
            return string.Join("\n", errors.Select(e => $"Error: {e.Error}, Context:{e.Context}"));
        }
    }
}
