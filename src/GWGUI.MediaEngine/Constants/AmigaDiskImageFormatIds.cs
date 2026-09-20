using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Définit les identifiants des images de disquettes Amiga.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe des formats Amiga.</summary>
    public const string AmigaPrefix = "amiga.";
    /// <summary>Image AmigaDOS double densité.</summary>
    public const string AmigaDos = MediaImageFormatIds.AmigaDos;
    /// <summary>Image AmigaDOS haute densité.</summary>
    public const string AmigaDosHighDensity = MediaImageFormatIds.AmigaDosHighDensity;
}
