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
            var errorColumnDefinition = new ColumnDefinition("Error",
                license => string.Join(Environment.NewLine, license.ValidationErrors.Select(e => e.Error)));
            var columnDefinitions = new[]
            {
                new ColumnDefinition("Package", license => license.PackageId, true),
                new ColumnDefinition("Version", license => license.PackageVersion.ToString() ,true),
                new ColumnDefinition("License Information Origin", license => license.LicenseInformationOrigin.ToString(), true),
                new ColumnDefinition("License Expression", license => license.License ?? string.Empty),
                new ColumnDefinition("Package Project Url",license => license.PackageProjectUrl??string.Empty),
                errorColumnDefinition,
                new ColumnDefinition("Error Context", license => string.Join(Environment.NewLine, license.ValidationErrors.Select(e => e.Context))),
            };

            foreach (var license in results)
            {
                foreach (var definition in columnDefinitions)
                {
                    definition.Enabled |= !string.IsNullOrWhiteSpace(definition.StringAccessor(license));
                }
            }

            if (_printErrorsOnly && errorColumnDefinition.Enabled)
            {
                results = results.Where(r => r.ValidationErrors.Any()).ToList();
            }

            var relevantColumns = columnDefinitions.Where(c => c.Enabled).ToArray();
            await TablePrinterExtensions
                .Create(stream, relevantColumns.Select(d => d.Title))
                .FromValues(
                    results,
                    license => relevantColumns.Select(d => d.StringAccessor(license)))
                .Print();
        }

        private record ColumnDefinition(string Title, Func<LicenseValidationResult, string> StringAccessor, bool Enabled = false)
        {
            public bool Enabled { get; set; } = Enabled;
        }
    }
}
