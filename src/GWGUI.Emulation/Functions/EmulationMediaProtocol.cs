using System.Text.Json;

namespace GWGUI.Emulation.Functions;

public static class EmulationMediaProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static byte[] Serialize(IReadOnlyList<EmulationMedia> media) =>
        JsonSerializer.SerializeToUtf8Bytes(EmulationMediaRules.Validate(media), JsonOptions);

    public static IReadOnlyList<EmulationMedia> Deserialize(ReadOnlySpan<byte> payload)
    {
        var media = JsonSerializer.Deserialize<EmulationMedia[]>(payload, JsonOptions)
            ?? throw new InvalidDataException(EmulationMediaErrorMessages.EmptyDocument);
        return EmulationMediaRules.Validate(media);
    }
}
