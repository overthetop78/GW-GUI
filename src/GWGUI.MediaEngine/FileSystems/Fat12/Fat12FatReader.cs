using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Sélectionne la copie de FAT12 la plus cohérente parmi celles annoncées par le BPB.</summary>
internal static class Fat12FatReader
{
    /// <summary>Indique si au moins une copie possède un en-tête compatible avec le descripteur du BPB.</summary>
    public static bool HasReadableCopy(SectorImage image, Fat12Layout layout, byte mediaDescriptor)
    {
        for (var copy = 0; copy < layout.FatCount; copy++)
        {
            var range = FatSectorReader.Read(image, layout.ReservedSectors + copy * layout.SectorsPerFat, layout.SectorsPerFat, []);
            if (range.IsValid && IsUsable(range.Bytes, mediaDescriptor, layout.ClusterCount)) return true;
        }
        return false;
    }

    /// <summary>Lit toutes les copies complètes et retient celle contenant le plus de chaînes allouées plausibles.</summary>
    public static FatSectorRange ReadBest(SectorImage image, Fat12Layout layout, byte mediaDescriptor, List<string> warnings)
    {
        var copies = new List<(FatSectorRange Range, int Score)>();
        for (var copy = 0; copy < layout.FatCount; copy++)
        {
            var copyWarnings = new List<string>();
            var range = FatSectorReader.Read(image, layout.ReservedSectors + copy * layout.SectorsPerFat, layout.SectorsPerFat, copyWarnings);
            if (!range.IsValid || !IsUsable(range.Bytes, mediaDescriptor, layout.ClusterCount)) continue;
            copies.Add((range, Score(range.Bytes, layout.ClusterCount)));
        }
        if (copies.Count == 0) return FatSectorReader.Read(image, layout.ReservedSectors, layout.SectorsPerFat, warnings);
        return copies.OrderByDescending(copy => copy.Score).First().Range;
    }

    /// <summary>Compte les entrées occupées dont la valeur appartient au domaine FAT12.</summary>
    private static int Score(ReadOnlySpan<byte> fat, int clusterCount)
    {
        var score = 0;
        for (var cluster = Fat12Table.FirstDataCluster; cluster < Fat12Table.FirstDataCluster + clusterCount; cluster++)
        {
            if (!Fat12Table.TryRead(fat, cluster, out var value) || value == Fat12Table.FreeCluster) continue;
            var dataCluster = value >= Fat12Table.FirstDataCluster && value < Fat12Table.FirstDataCluster + clusterCount;
            var terminalOrReserved = value >= Fat12Table.FirstReservedCluster && value <= Fat12Table.LastEndOfChain;
            if (dataCluster || terminalOrReserved) score++;
        }
        return score;
    }

    /// <summary>Valide l'en-tête standard, ou exige des allocations réelles lorsque l'ancien en-tête nul est employé.</summary>
    public static bool IsUsable(ReadOnlySpan<byte> fat, byte mediaDescriptor, int clusterCount) =>
        Fat12Table.HasPlausibleHeader(fat) ||
        mediaDescriptor == FatBootSectorLayout.UnknownMediaDescriptor && fat.Length >= 3 && fat[0] == 0 && fat[1] == 0 && fat[2] == 0 && Score(fat, clusterCount) > 0;
}
