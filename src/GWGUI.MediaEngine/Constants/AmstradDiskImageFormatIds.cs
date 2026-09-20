using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Définit les identifiants des images de disquettes Amstrad.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe des formats Amstrad.</summary>
    public const string AmstradPrefix = "amstrad.";
    /// <summary>Image sectorielle Amstrad CPC.</summary>
    public const string AmstradCpc = MediaImageFormatIds.AmstradCpc;
    /// <summary>Image sectorielle Amstrad PCW.</summary>
    public const string AmstradPcw = MediaImageFormatIds.AmstradPcw;
}
