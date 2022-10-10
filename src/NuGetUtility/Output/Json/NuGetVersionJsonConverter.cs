using NuGet.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Output.Json
{
    public class NuGetVersionJsonConverter : JsonConverter<NuGetVersion>
    {
        public override NuGetVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readStringValue = reader.GetString();
            return new NuGetVersion(readStringValue);
        }

        public override void Write(Utf8JsonWriter writer, NuGetVersion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToNormalizedString());
        }
    }
}
