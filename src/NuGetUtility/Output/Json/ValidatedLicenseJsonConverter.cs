using NuGetUtility.LicenseValidator;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Output.Json
{
    internal class ValidatedLicenseJsonConverter : JsonConverter<ValidatedLicense>
    {
        private bool _withPackageUrls;

        public ValidatedLicenseJsonConverter(bool withPackageUrls) => _withPackageUrls = withPackageUrls;

        public override ValidatedLicense? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<ValidatedLicense?>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, ValidatedLicense value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ConvertToLicenseWithout(_withPackageUrls ? Array.Empty<string>() : new string[] { nameof(ValidatedLicense.ProjectUrl) }), options);
        }


    }

    internal static class ValidatedLicenseExtensions
    {
        public static object ConvertToLicenseWithout(
                     this ValidatedLicense license,
                     string[] propertiesToIgnore)
        {
            if (license == null)
                throw new ArgumentNullException(nameof(license));

            var newLicense = new ExpandoObject() as IDictionary<string, object>;

            foreach (var propertyInfo in typeof(ValidatedLicense).GetProperties())
                if (!propertiesToIgnore.Contains(propertyInfo.Name))
                    newLicense.Add(propertyInfo.Name,
                                    propertyInfo.GetValue(license)!);

            return newLicense;
        }
    }
}
