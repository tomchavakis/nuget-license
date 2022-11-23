using NuGetUtility.LicenseValidator;
using NuGetUtility.Serialization;
using System.Text.Json;

namespace NuGetUtility.Output.Json
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        private readonly bool _printErrorsOnly;
        private readonly bool _skipIgnoredPackages;
        private readonly JsonSerializerOptions _options;
        public JsonOutputFormatter(bool prettyPrint, bool printErrorsOnly, bool skipIgnoredPackages)
        {
            _printErrorsOnly = printErrorsOnly;
            _skipIgnoredPackages = skipIgnoredPackages;
            _options = new JsonSerializerOptions
            {
                Converters = { new NuGetVersionJsonConverter(), new ValidatedLicenseJsonConverterWithOmittingEmptyErrorList() },
                WriteIndented = prettyPrint
            };
        }

        public async Task Write(Stream stream, IList<LicenseValidationResult> results)
        {
            if (_printErrorsOnly)
            {
                IEnumerable<LicenseValidationResult> resultsWithErrors = results.Where(r => r.ValidationErrors.Any());
                if (resultsWithErrors.Any())
                {
                    await JsonSerializer.SerializeAsync(stream, resultsWithErrors, _options);
                    return;
                }
            }

            if (_skipIgnoredPackages)
            {
                results.Where(r => r.LicenseInformationOrigin != LicenseInformationOrigin.Ignored);
            }

            await JsonSerializer.SerializeAsync(stream, results, _options);
        }
    }
}
