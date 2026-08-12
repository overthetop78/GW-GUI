using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Valide un BPB FAT12 ou une disposition IBM historique.</summary>
internal static class Fat12LayoutReader
{
    /// <summary>Tente de construire la disposition calculée correspondant au secteur d'amorçage.</summary>
    public static bool TryRead(ReadOnlySpan<byte> boot, int availableSectors, string formatId, out Fat12Layout layout)
    {
        layout = null!;
        var bytesPerSector = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBootSectorLayout.BytesPerSectorOffset..]);
        var sectorsPerCluster = boot[FatBootSectorLayout.SectorsPerClusterOffset];
        var reservedSectors = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBootSectorLayout.ReservedSectorCountOffset..]);
        var fatCount = boot[FatBootSectorLayout.FatCountOffset];
        var rootEntries = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBootSectorLayout.RootEntryCountOffset..]);
        var totalSectors = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBootSectorLayout.TotalSectors16Offset..]);
        if (totalSectors == 0)
        {
            var totalSectors32 = BinaryPrimitives.ReadUInt32LittleEndian(boot[FatBootSectorLayout.TotalSectors32Offset..]);
            if (totalSectors32 > ushort.MaxValue) return false;
            totalSectors = (ushort)totalSectors32;
        }
        var sectorsPerFat = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBootSectorLayout.SectorsPerFatOffset..]);
        if (bytesPerSector != FatBootSectorLayout.SectorSize || sectorsPerCluster == 0 || reservedSectors == 0 || fatCount == 0 || rootEntries == 0 || totalSectors == 0 || totalSectors > availableSectors || sectorsPerFat == 0) return Fat12LegacyLayoutCatalog.TryCreateLayout(formatId, availableSectors, boot, out layout);
        var rootSectors = FatBootSectorLayout.RootDirectorySectorCount(rootEntries);
        var rootStart = reservedSectors + fatCount * sectorsPerFat;
        var dataStart = rootStart + rootSectors;
        if (dataStart >= totalSectors) return false;
        var clusterCount = (totalSectors - dataStart) / sectorsPerCluster;
        if (clusterCount is <= 0 or >= Fat12Layout.MaximumClusterCount) return false;
        layout = new(reservedSectors, sectorsPerFat, rootStart, rootSectors, dataStart, sectorsPerCluster, clusterCount, fatCount);
        return true;
    }
}
