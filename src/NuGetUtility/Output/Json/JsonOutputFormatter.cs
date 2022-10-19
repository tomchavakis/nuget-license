using NuGetUtility.LicenseValidator;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Output.Json
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        private readonly JsonSerializerOptions _options;
        public JsonOutputFormatter(bool prettyPrint = false)
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new NuGetVersionJsonConverter() },
                WriteIndented = prettyPrint,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task Write(Stream stream, IList<LicenseValidationResult> results)
        {
            await JsonSerializer.SerializeAsync(stream, results, _options);
        }
    }
}
