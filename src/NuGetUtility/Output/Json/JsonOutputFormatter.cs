using NuGetUtility.LicenseValidator;
using System.Text.Json;

namespace NuGetUtility.Output.Json
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        private readonly bool _printErrorsOnly;
        private readonly JsonSerializerOptions _options;
        public JsonOutputFormatter(bool prettyPrint = false, bool printErrorsOnly = false)
        {
            _printErrorsOnly = printErrorsOnly;
            _options = new JsonSerializerOptions
            {
                Converters =
                    { new NuGetVersionJsonConverter(), new ValidatedLicenseJsonConverterWithOmittingEmptyErrorList() },
                WriteIndented = prettyPrint
            };
        }

        public async Task Write(Stream stream, IList<LicenseValidationResult> results)
        {
            if (_printErrorsOnly)
            {
                var resultsWithErrors = results.Where(r => r.ValidationErrors.Any()).ToList();
                if (resultsWithErrors.Any())
                {
                    await JsonSerializer.SerializeAsync(stream, resultsWithErrors, _options);
                    return;
                }
            }

            await JsonSerializer.SerializeAsync(stream, results, _options);
        }
    }
}
