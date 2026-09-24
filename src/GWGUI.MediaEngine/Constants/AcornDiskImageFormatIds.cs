using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Définit les identifiants des images de disquettes Acorn.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe des formats Acorn ADFS.</summary>
    public const string AcornAdfsPrefix = "acorn.adfs.";
    /// <summary>Image Acorn ADFS de 800 Kio.</summary>
    public const string AcornAdfs800 = MediaImageFormatIds.AcornAdfs800;
    /// <summary>Image Acorn ADFS de 1600 Kio.</summary>
    public const string AcornAdfs1600 = "acorn.adfs.1600";
    /// <summary>Préfixe des formats Acorn DFS.</summary>
    public const string AcornDfsPrefix = "acorn.dfs.";
    /// <summary>Image Acorn Atom DOS simple face de 40 pistes.</summary>
    public const string AcornAtomDos = MediaImageFormatIds.AcornAtomDos;
    /// <summary>Image Acorn DFS simple face de 40 pistes.</summary>
    public const string AcornDfsSingleSided = MediaImageFormatIds.AcornDfsSingleSided;
    /// <summary>Image Acorn DFS simple face de 80 pistes.</summary>
    public const string AcornDfsSingleSided80 = MediaImageFormatIds.AcornDfsSingleSided80;
    /// <summary>Image Acorn DFS double face de 40 pistes.</summary>
    public const string AcornDfsDoubleSided = MediaImageFormatIds.AcornDfsDoubleSided;
    /// <summary>Image Acorn DFS double face de 80 pistes.</summary>
    public const string AcornDfsDoubleSided80 = MediaImageFormatIds.AcornDfsDoubleSided80;
}
