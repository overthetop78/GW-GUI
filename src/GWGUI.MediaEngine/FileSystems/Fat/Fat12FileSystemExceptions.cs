namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Construit les diagnostics propres aux volumes FAT12.</summary>
public static class Fat12FileSystemExceptions
{
    /// <summary>Crée l'erreur signalant une disposition non prise en charge.</summary>
    public static InvalidDataException UnsupportedLayout(string formatId, IReadOnlyList<byte>? boot)
    {
        if (boot is null || boot.Count < FatBpbLayout.MinimumLength) return new($"The image format '{formatId}' does not contain a supported FAT12 file system; its boot sector is unavailable or truncated.");
        var bytes = boot.ToArray();
        var sectorSize = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBpbLayout.BytesPerSectorOffset));
        var sectorsPerCluster = bytes[FatBpbLayout.SectorsPerClusterOffset];
        var reserved = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBpbLayout.ReservedSectorCountOffset));
        var fatCount = bytes[FatBpbLayout.FatCountOffset];
        var rootEntries = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBpbLayout.RootEntryCountOffset));
        var fatSectors = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(FatBpbLayout.SectorsPerFatOffset));
        return new($"The image format '{formatId}' does not contain a supported FAT12 file system (sector size {sectorSize}, sectors/cluster {sectorsPerCluster}, reserved {reserved}, FAT count {fatCount}, root entries {rootEntries}, sectors/FAT {fatSectors}).");
    }
    /// <summary>Crée l'erreur signalant une chaîne invalide ou cyclique.</summary>
    public static InvalidDataException InvalidChain(string name, int cluster) => new($"Invalid or cyclic FAT chain for '{name}' at cluster {cluster}.");
    /// <summary>Crée l'erreur signalant une entrée FAT tronquée.</summary>
    public static InvalidDataException TruncatedTable(int cluster) => new($"The FAT12 entry for cluster {cluster} is truncated.");
    /// <summary>Construit l'avertissement signalant des secteurs absents.</summary>
    public static string MissingSectors(int firstSector, int count) => $"Sector range {firstSector}..{firstSector + count - 1} is incomplete.";
    /// <summary>Construit l'avertissement signalant la limite de profondeur.</summary>
    public static string DepthLimit(string path, int depth) => $"The FAT directory nesting limit was reached at '{path}' (depth {depth}).";
}
