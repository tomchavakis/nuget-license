using NuGetUtility.LicenseValidator;
using System.Text.Json;

namespace NuGetUtility.Output.Json
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        private readonly JsonSerializerOptions _options;
        public JsonOutputFormatter(bool withPackageUrls, bool prettyPrint = false)
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new NuGetVersionJsonConverter(), new ValidatedLicenseJsonConverter(withPackageUrls) },
                WriteIndented = prettyPrint
            };
        }

        public async Task Write(Stream stream, IEnumerable<LicenseValidationError> errors)
        {
            await JsonSerializer.SerializeAsync(stream, errors, _options);
        }
        public async Task Write(Stream stream, IEnumerable<ValidatedLicense> validated)
        {
            await JsonSerializer.SerializeAsync(stream, validated, _options);
        }
    }
}
