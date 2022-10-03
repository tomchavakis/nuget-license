using NuGetUtility.LicenseValidator;
using System.Text.Json;

namespace NuGetUtility.Output.Json
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        private readonly JsonSerializerOptions _options;
        public JsonOutputFormatter()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new NuGetVersionJsonConverter() }
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
