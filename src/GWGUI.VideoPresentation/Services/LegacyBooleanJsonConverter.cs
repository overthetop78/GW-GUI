using System.Text.Json;
using System.Text.Json.Serialization;

namespace GWGUI.VideoPresentation.Services;

internal sealed class LegacyBooleanJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt32(out var number) && number is 0 or 1 => number == 1,
            _ => throw new JsonException("Invalid boolean in video profile.")
        };
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value);
}
