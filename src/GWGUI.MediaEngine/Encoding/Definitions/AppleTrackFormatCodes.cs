namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Définit les codes de format transmis aux encodeurs Apple.</summary>
internal static class AppleTrackFormatCodes
{
    /// <summary>Format Apple II standard.</summary>
    public const int AppleII = 0;
    /// <summary>Format Apple II ProDOS quatre-vingts pistes.</summary>
    public const int AppleIIProDos80Track = 0x24;
    /// <summary>Format Macintosh simple face.</summary>
    public const int MacintoshSingleSided = 0x02;
    /// <summary>Format Macintosh double face.</summary>
    public const int MacintoshDoubleSided = 0x22;
    /// <summary>Format Lisa FileWare.</summary>
    public const int LisaFileWare = 0x12;
}
