using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Acorn;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Definitions;

/// <summary>Catalogue immuable des formats candidats associés à une taille de bloc.</summary>
internal static class CompatibleFormatCatalog
{
    /// <summary>Taille des blocs ISO/FAT.</summary>
    public const int IsoBlockSize = FatBootSectorLayout.SectorSize;
    /// <summary>Taille des blocs Acorn DFS.</summary>
    public const int DfsBlockSize = BbcDfsGeometry.SectorSize;
    /// <summary>Taille des blocs Acorn ADFS.</summary>
    public const int AdfsBlockSize = AcornAdfGeometry.BlockSize;
    private static readonly IReadOnlyList<string> IsoFormats = Array.AsReadOnly(new[] { DiskImageFormatIds.UcsdIbmMfm, DiskImageFormatIds.Commodore900Coherent, DiskImageFormatIds.EpsonQx10_396, DiskImageFormatIds.EpsonQx10_399, DiskImageFormatIds.EpsonQx10Logo });
    private static readonly IReadOnlyList<string> DfsFormats = Array.AsReadOnly(new[] { DiskImageFormatIds.AcornDfsSingleSided, DiskImageFormatIds.AcornDfsSingleSided80, DiskImageFormatIds.AcornDfsDoubleSided, DiskImageFormatIds.AcornDfsDoubleSided80, DiskImageFormatIds.EpsonQx10_320 });
    private static readonly IReadOnlyList<string> AdfsFormats = Array.AsReadOnly(new[] { DiskImageFormatIds.EpsonQx10_400 });

    /// <summary>Retourne la liste immuable des candidats, ou une collection vide pour une taille inconnue.</summary>
    public static IReadOnlyList<string> Resolve(int blockSize) => blockSize switch { IsoBlockSize => IsoFormats, DfsBlockSize => DfsFormats, AdfsBlockSize => AdfsFormats, _ => [] };
}
