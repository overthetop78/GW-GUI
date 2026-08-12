namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Décode les dates Unix du système COHERENT.</summary>
public static class CoherentFileSystemTime
{
    /// <summary>Retourne une date absente pour zéro ou une valeur hors plage.</summary>
    public static DateTimeOffset? Decode(uint seconds)
    {
        if (seconds == 0) return null;
        try { return DateTimeOffset.FromUnixTimeSeconds(seconds); }
        catch (ArgumentOutOfRangeException) { return null; }
    }
}
