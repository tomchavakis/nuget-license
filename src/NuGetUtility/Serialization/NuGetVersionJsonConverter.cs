using NuGetUtility.Wrapper.NuGetWrapper.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGetUtility.Serialization
{
    internal class NuGetVersionJsonConverter : JsonConverter<INuGetVersion>
    {
        public override INuGetVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string stringVersion = JsonSerializer.Deserialize<string>(ref reader, options)!;
            if (WrappedNuGetVersion.TryParse(stringVersion, out WrappedNuGetVersion? version))
            {
                return version;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, INuGetVersion value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToString(), options);
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(INuGetVersion));
        }
    }
}
