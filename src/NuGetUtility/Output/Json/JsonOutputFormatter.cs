using NuGetUtility.LicenseValidator;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Output.Json
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        private readonly bool _omitValidLicensesIfErrorsExist;
        private readonly JsonSerializerOptions _options;
        public JsonOutputFormatter(bool prettyPrint = false, bool omitValidLicensesIfErrorsExist = false)
        {
            _omitValidLicensesIfErrorsExist = omitValidLicensesIfErrorsExist;
            _options = new JsonSerializerOptions
            {
                Converters = { new NuGetVersionJsonConverter(), new ValidatedLicenseJsonConverterWithOmittingEmptyErrorList() },
                WriteIndented = prettyPrint
            };
        }

        public async Task Write(Stream stream, IList<LicenseValidationResult> results)
        {
            if (_omitValidLicensesIfErrorsExist)
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
