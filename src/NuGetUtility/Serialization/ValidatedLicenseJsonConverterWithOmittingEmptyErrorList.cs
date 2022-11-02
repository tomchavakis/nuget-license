using NuGetUtility.LicenseValidator;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Serialization
{
    public class ValidatedLicenseJsonConverterWithOmittingEmptyErrorList : JsonConverter<LicenseValidationResult>
    {
        public override LicenseValidationResult? Read(ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        public override void Write(Utf8JsonWriter writer, LicenseValidationResult value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var propertyInfo in value.GetType().GetProperties())
            {
                if (propertyInfo.Name == nameof(value.ValidationErrors))
                {
                    if (!value.ValidationErrors.Any())
                    {
                        continue;
                    }
                }
                var writeValue = propertyInfo.GetValue(value);
                if (writeValue != null)
                {
                    writer.WritePropertyName(propertyInfo.Name);
                    JsonSerializer.Serialize(writer, propertyInfo.GetValue(value), options);
                }
            }
            writer.WriteEndObject();
        }
    }
}
