// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Text.Json;
using NuGetUtility.LicenseValidator;
using NuGetUtility.Serialization;

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
                results = results.Where(r => r.ValidationErrors.Any()).ToList();
            }
            else if (_skipIgnoredPackages)
            {
                results = results.Where(r => r.LicenseInformationOrigin != LicenseInformationOrigin.Ignored).ToList();
            }

            await JsonSerializer.SerializeAsync(stream, results, _options);
        }
    }
}
