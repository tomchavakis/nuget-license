using NuGet.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Serialization
{
    internal class NuGetVersionConverter : JsonConverter<NuGetVersion>
    {
        public override NuGetVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringVersion = JsonSerializer.Deserialize<string>(ref reader, options)!;
            if (NuGetVersion.TryParse(stringVersion, out var version))
            {
                return version;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, NuGetVersion value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToString(), options);
        }
    }
}
