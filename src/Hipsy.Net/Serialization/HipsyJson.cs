using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hipsy.Net.Serialization;

/// <summary>JSON settings that match the Hipsy API (snake_case, prices as strings, tolerant enums).</summary>
internal static class HipsyJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new TolerantEnumConverterFactory() },
    };
}

/// <summary>
/// Reads snake_case enum values (for example <c>start_future</c>) and maps values it doesn't know
/// to the enum's 0 value (Unknown), so a new value on Hipsy's side doesn't break deserialization.
/// </summary>
internal sealed class TolerantEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(TolerantEnumConverter<>).MakeGenericType(typeToConvert))!;

    private sealed class TolerantEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private static readonly Dictionary<string, T> ByName = Enum.GetValues<T>()
            .ToDictionary(v => JsonNamingPolicy.SnakeCaseLower.ConvertName(v.ToString()), StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<T, string> ToName = ByName.ToDictionary(kv => kv.Value, kv => kv.Key);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && ByName.TryGetValue(reader.GetString()!, out var value))
            {
                return value;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                reader.Skip();
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(ToName.TryGetValue(value, out var name) ? name : value.ToString());
    }
}
