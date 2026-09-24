using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;
using System.Collections.Frozen;

namespace GWGUI.MediaFileSystems.FileSystems.Fat12;

/// <summary>Associe explicitement chaque format sectoriel pris en charge au système FAT12.</summary>
public static class Fat12FormatCatalog
{
    /// <summary>Association immuable des formats Atari ST, IBM PC et MSX vers l'identifiant FAT12.</summary>
    public static IReadOnlyDictionary<string, string> FileSystemIdByFormat { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [MediaImageFormatIds.AtariSt180] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt360] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt400] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt440] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt720] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt800] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt810] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt880] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.AtariSt1440] = Definitions.FileSystemIds.Fat12,
        [MediaImageFormatIds.Ibm160] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm180] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm320] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm360] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm720] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm800] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm1200] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm1440] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm1680] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.IbmDmf] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Ibm2880] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.IbmScan] = Definitions.FileSystemIds.Fat12,
        [MediaImageFormatIds.Msx1D] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Msx1Dd] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Msx2D] = Definitions.FileSystemIds.Fat12, [MediaImageFormatIds.Msx2Dd] = Definitions.FileSystemIds.Fat12,
        [MediaImageFormatIds.ApricotPcXi315] = Definitions.FileSystemIds.Fat12
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
