using NuGetUtility.Wrapper.NuGetWrapper.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Output.Json
{
    public class NuGetVersionJsonConverter : JsonConverter<INuGetVersion>
    {
        public override INuGetVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readStringValue = reader.GetString();
            return new WrappedNuGetVersion(readStringValue!);
        }

        public override void Write(Utf8JsonWriter writer, INuGetVersion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
