using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Associe explicitement chaque format sectoriel pris en charge au système FAT12.</summary>
public static class Fat12FormatCatalog
{
    /// <summary>Association immuable des formats Atari ST, IBM PC et MSX vers l'identifiant FAT12.</summary>
    public static IReadOnlyDictionary<string, string> FileSystemIdByFormat { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [DiskImageFormatIds.AtariSt180] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt360] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt400] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt440] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt720] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt800] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt810] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt880] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.AtariSt1440] = Definitions.FileSystemIds.Fat12,
        [DiskImageFormatIds.Ibm160] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm180] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm320] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm360] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm720] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm800] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm1200] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm1440] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm1680] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.IbmDmf] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Ibm2880] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.IbmScan] = Definitions.FileSystemIds.Fat12,
        [DiskImageFormatIds.Msx1D] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Msx1Dd] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Msx2D] = Definitions.FileSystemIds.Fat12, [DiskImageFormatIds.Msx2Dd] = Definitions.FileSystemIds.Fat12
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
