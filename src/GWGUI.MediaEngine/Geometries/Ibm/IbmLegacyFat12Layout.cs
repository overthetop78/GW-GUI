using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Geometries.Ibm;

/// <summary>Décrit les paramètres FAT12 d'une image IBM historique dépourvue de BPB exploitable.</summary>
public sealed record IbmLegacyFat12Layout(string FormatId, int TotalSectors, int SectorsPerCluster, int RootEntries, int SectorsPerFat)
{
    /// <summary>Catalogue immuable des quatre dispositions historiques.</summary>
    public static IReadOnlyDictionary<string, IbmLegacyFat12Layout> ByFormatId { get; } = new System.Collections.ObjectModel.ReadOnlyDictionary<string, IbmLegacyFat12Layout>(new Dictionary<string, IbmLegacyFat12Layout>(StringComparer.OrdinalIgnoreCase)
    {
        [DiskImageFormatIds.Ibm160] = new(DiskImageFormatIds.Ibm160, 320, 1, 64, 1),
        [DiskImageFormatIds.Ibm180] = new(DiskImageFormatIds.Ibm180, 360, 1, 64, 2),
        [DiskImageFormatIds.Ibm320] = new(DiskImageFormatIds.Ibm320, 640, 2, 112, 1),
        [DiskImageFormatIds.Ibm360] = new(DiskImageFormatIds.Ibm360, 720, 2, 112, 2)
    });

    /// <summary>Recherche une disposition par identifiant de format.</summary>
    public static bool TryResolve(string formatId, out IbmLegacyFat12Layout layout) => ByFormatId.TryGetValue(formatId, out layout!);
}
