using NuGetUtility.LicenseValidator;

namespace NuGetUtility.Output.Table
{
    public class TableOutputFormatter : IOutputFormatter
    {
        private readonly bool _printErrorsOnly;
        public TableOutputFormatter(bool printErrorsOnly = false)
        {
            _printErrorsOnly = printErrorsOnly;
        }

        public async Task Write(Stream stream, IList<LicenseValidationResult> results)
        {
            var printPackageProjectUrl = false;
            var hasPackagesWithErrors = false;
            foreach (var license in results)
            {
                printPackageProjectUrl |= license.PackageProjectUrl != null;
                hasPackagesWithErrors |= license.ValidationErrors.Any();
            }
            var headings = new List<string>(6)
                { "Package", "Version", "License Information Origin", "License Expression" };
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
                headings.Add("Error");
                headings.Add("Error Context");
                formatters.Add(license =>
                    string.Join(Environment.NewLine, license.ValidationErrors.Select(e => e.Error)));
                formatters.Add(license =>
                    string.Join(Environment.NewLine, license.ValidationErrors.Select(e => e.Context)));

                if (_printErrorsOnly)
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
    }
}
