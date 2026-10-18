namespace GWGUI.MediaFileSystems.Functions;

/// <summary>Reads decimal integer values from media metadata.</summary>
public static class MediaMetadataFunctions
{
    public static bool TryReadInt(IReadOnlyDictionary<string, string> metadata, string key, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value);
    }
}
