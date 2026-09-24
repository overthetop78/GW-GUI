using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaFileSystems.FileSystems.Fat12;

/// <summary>Décrit et catalogue les paramètres FAT12 d'une image IBM historique dépourvue de BPB exploitable.</summary>
public sealed record Fat12LegacyLayoutCatalog
{
    /// <summary>Nombre de secteurs réservés commun aux dispositions historiques.</summary>
    public const int ReservedSectorCount = 1;
    /// <summary>Nombre de copies de FAT commun aux dispositions historiques.</summary>
    public const int FatCopyCount = 2;

    /// <summary>Crée une disposition historique nommée.</summary>
    public Fat12LegacyLayoutCatalog(string formatId, int totalSectors, int sectorsPerCluster, int rootEntries, int sectorsPerFat)
    {
        FormatId = formatId;
        TotalSectors = totalSectors;
        SectorsPerCluster = sectorsPerCluster;
        RootEntries = rootEntries;
        SectorsPerFat = sectorsPerFat;
    }

    /// <summary>Identifiant du format sectoriel.</summary>
    public string FormatId { get; }
    /// <summary>Nombre total de secteurs.</summary>
    public int TotalSectors { get; }
    /// <summary>Nombre de secteurs par cluster.</summary>
    public int SectorsPerCluster { get; }
    /// <summary>Nombre d'entrées du répertoire racine.</summary>
    public int RootEntries { get; }
    /// <summary>Nombre de secteurs par copie de FAT.</summary>
    public int SectorsPerFat { get; }

    /// <summary>Catalogue immuable des quatre dispositions historiques.</summary>
    public static IReadOnlyDictionary<string, Fat12LegacyLayoutCatalog> ByFormatId { get; } = new System.Collections.ObjectModel.ReadOnlyDictionary<string, Fat12LegacyLayoutCatalog>(new Dictionary<string, Fat12LegacyLayoutCatalog>(StringComparer.OrdinalIgnoreCase)
    {
        [MediaImageFormatIds.Ibm160] = new(MediaImageFormatIds.Ibm160, 320, 1, 64, 1),
        [MediaImageFormatIds.Ibm180] = new(MediaImageFormatIds.Ibm180, 360, 1, 64, 2),
        [MediaImageFormatIds.Ibm320] = new(MediaImageFormatIds.Ibm320, 640, 2, 112, 1),
        [MediaImageFormatIds.Ibm360] = new(MediaImageFormatIds.Ibm360, 720, 2, 112, 2),
        [MediaImageFormatIds.ApricotPcXi315] = new(MediaImageFormatIds.ApricotPcXi315, 630, 1, 64, 2)
    });

    /// <summary>Recherche une disposition par identifiant de format.</summary>
    public static bool TryResolve(string formatId, out Fat12LegacyLayoutCatalog layout) => ByFormatId.TryGetValue(formatId, out layout!);

    /// <summary>Tente de construire directement une disposition validée après contrôle du secteur non uniforme et de la capacité.</summary>
    public static bool TryCreateLayout(string formatId, int availableSectors, ReadOnlySpan<byte> boot, out Fat12Layout layout)
    {
        layout = null!;
        if (boot.Length == 0 || boot.IndexOfAnyExcept(boot[0]) < 0 || !TryResolve(formatId, out var legacy) || availableSectors < legacy.TotalSectors) return false;
        var rootSectors = FatBootSectorLayout.RootDirectorySectorCount(legacy.RootEntries);
        var rootStart = ReservedSectorCount + FatCopyCount * legacy.SectorsPerFat;
        var dataStart = rootStart + rootSectors;
        var clusters = (legacy.TotalSectors - dataStart) / legacy.SectorsPerCluster;
        layout = new(ReservedSectorCount, legacy.SectorsPerFat, rootStart, rootSectors, dataStart, legacy.SectorsPerCluster, clusters);
        return true;
    }
}
