using System.Globalization;

namespace GWGUI.MediaFileSystems.Functions;

/// <summary>Reads decimal integer values from media metadata.</summary>
public static class MediaMetadataFunctions
{
    public static bool TryReadInt(IReadOnlyDictionary<string, string> metadata, string key, out int value)
        => TryReadInt(metadata, key, NumberStyles.None, out value);

    public static bool TryReadInt(IReadOnlyDictionary<string, string> metadata, string key, NumberStyles style, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, style, CultureInfo.InvariantCulture, out value);
    }

    public static int ReadIntOrDefault(IReadOnlyDictionary<string, string> metadata, string key)
        => TryReadInt(metadata, key, NumberStyles.Integer, out var value) ? value : 0;
}
