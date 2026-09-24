using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Définit les identifiants des images de disquettes Commodore.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe des formats Commodore.</summary>
    public const string CommodorePrefix = "commodore.";
    /// <summary>Image Commodore 1541.</summary>
    public const string Commodore1541 = MediaImageFormatIds.Commodore1541;
    /// <summary>Image Commodore 1571.</summary>
    public const string Commodore1571 = MediaImageFormatIds.Commodore1571;
    /// <summary>Image Commodore 1581.</summary>
    public const string Commodore1581 = MediaImageFormatIds.Commodore1581;
    /// <summary>Préfixe des formats Commodore 900.</summary>
    public const string Commodore900Prefix = "commodore900.";
    /// <summary>Image Commodore 900 utilisant Coherent.</summary>
    public const string Commodore900Coherent = MediaImageFormatIds.Commodore900Coherent;
}
