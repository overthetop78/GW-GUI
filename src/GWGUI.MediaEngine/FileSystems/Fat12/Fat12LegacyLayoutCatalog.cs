using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

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
        [DiskImageFormatIds.Ibm160] = new(DiskImageFormatIds.Ibm160, 320, 1, 64, 1),
        [DiskImageFormatIds.Ibm180] = new(DiskImageFormatIds.Ibm180, 360, 1, 64, 2),
        [DiskImageFormatIds.Ibm320] = new(DiskImageFormatIds.Ibm320, 640, 2, 112, 1),
        [DiskImageFormatIds.Ibm360] = new(DiskImageFormatIds.Ibm360, 720, 2, 112, 2)
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
