using System.Text.Json;

namespace GWGUI.Emulation.Functions;

internal static class EmulationMediaProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static byte[] Serialize(IReadOnlyList<EmulationMedia> media) => JsonSerializer.SerializeToUtf8Bytes(EmulationSdk.MediaRules.Validate(media), JsonOptions);

    internal static IReadOnlyList<EmulationMedia> Deserialize(ReadOnlySpan<byte> payload)
    {
        var media = JsonSerializer.Deserialize<EmulationMedia[]>(payload, JsonOptions) ?? throw new InvalidDataException(EmulationMediaErrorMessages.EmptyDocument);
        return EmulationSdk.MediaRules.Validate(media);
    }
}
