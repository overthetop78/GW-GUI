using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les volumes FAT12 contenus dans les images Atari ST, IBM PC et MSX.</summary>
public sealed class Fat12FileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.Fat12;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        DiskImageFormatIds.AtariSt180, DiskImageFormatIds.AtariSt360, DiskImageFormatIds.AtariSt400, DiskImageFormatIds.AtariSt440, DiskImageFormatIds.AtariSt720, DiskImageFormatIds.AtariSt800, DiskImageFormatIds.AtariSt810, DiskImageFormatIds.AtariSt880, DiskImageFormatIds.AtariSt1440,
        DiskImageFormatIds.Ibm160, DiskImageFormatIds.Ibm180, DiskImageFormatIds.Ibm320, DiskImageFormatIds.Ibm360, DiskImageFormatIds.Ibm720, DiskImageFormatIds.Ibm800, DiskImageFormatIds.Ibm1200, DiskImageFormatIds.Ibm1440, DiskImageFormatIds.Ibm1680, DiskImageFormatIds.IbmDmf, DiskImageFormatIds.Ibm2880, DiskImageFormatIds.IbmScan,
        DiskImageFormatIds.Msx1D, DiskImageFormatIds.Msx1Dd, DiskImageFormatIds.Msx2D, DiskImageFormatIds.Msx2Dd
    };

    /// <inheritdoc />
    public bool CanRead(SectorImage image)
    {
        if (image.BlockSize != FatBpbLayout.SectorSize || !image.TryGetBlock(0, out var boot) || boot.Data.Count < FatBpbLayout.ExtendedBootMinimumLength) return false;
        return TryReadLayout(boot.Data.ToArray(), image.BlockCount, image.FormatId, out var layout) && HasPlausibleFatHeader(image, layout);
    }

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!image.TryGetBlock(0, out var boot)) throw Fat12FileSystemExceptions.UnsupportedLayout(image.FormatId, null);
        if (!TryReadLayout(boot.Data.ToArray(), image.BlockCount, image.FormatId, out var layout) || !HasPlausibleFatHeader(image, layout)) throw Fat12FileSystemExceptions.UnsupportedLayout(image.FormatId, boot.Data);
        var warnings = new List<string>();
        var fat = ReadSectors(image, layout.ReservedSectors, layout.SectorsPerFat, warnings);
        var root = ReadSectors(image, layout.RootStart, layout.RootSectors, warnings);
        var entries = ReadDirectory(image, root, fat, layout, warnings, 0, string.Empty, new HashSet<int>());
        var freeClusters = Enumerable.Range(Fat12Layout.FirstDataCluster, Math.Max(0, layout.ClusterCount - Fat12Layout.FirstDataCluster)).Count(cluster => Fat12Table.TryRead(fat, cluster, out var value) && value == Fat12Table.FreeCluster);
        var label = ReadVolumeLabel(root) ?? ReadBootVolumeLabel(boot.Data);
        return new(label, Definitions.FileSystemIds.Fat12, image.Capacity, (long)freeClusters * layout.SectorsPerCluster * FatBpbLayout.SectorSize, null, null, entries, warnings);
    }

    /// <summary>Lit le label de volume présent dans le secteur d'amorçage.</summary>
    private static string ReadBootVolumeLabel(IReadOnlyList<byte> boot)
    {
        if (boot.Count < FatBpbLayout.ExtendedBootMinimumLength) return string.Empty;
        var bytes = boot.Skip(FatBpbLayout.VolumeLabelOffset).Take(FatBpbLayout.VolumeLabelLength).ToArray();
        if (bytes.Any(value => value is < 0x20 or > 0x7e)) return string.Empty;
        var label = System.Text.Encoding.ASCII.GetString(bytes).Trim();
        return label.Equals("NO NAME", StringComparison.OrdinalIgnoreCase) ? string.Empty : label;
    }

    /// <summary>Lit récursivement un répertoire FAT12.</summary>
    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, byte[] directory, byte[] fat, Fat12Layout layout, List<string> warnings, int depth, string path, HashSet<int> visited)
    {
        if (depth > 64) { warnings.Add(Fat12FileSystemExceptions.DepthLimit(path, depth)); return []; }
        var entries = new List<FileSystemEntry>();
        for (var offset = 0; offset + FatDirectoryLayout.EntrySize <= directory.Length; offset += FatDirectoryLayout.EntrySize)
        {
            var first = directory[offset];
            if (first == FatDirectoryLayout.EndMarker) break;
            if (first == FatDirectoryLayout.DeletedMarker) continue;
            var attributes = (FatDirectoryAttributes)directory[offset + FatDirectoryLayout.AttributesOffset];
            if ((attributes & FatDirectoryLayout.LongFileName) == FatDirectoryLayout.LongFileName || attributes.HasFlag(FatDirectoryAttributes.VolumeLabel)) continue;
            var name = DecodeName(directory.AsSpan(offset, FatDirectoryLayout.ExtensionOffset + FatDirectoryLayout.ExtensionLength));
            if (name is "." or ".." || name.Length == 0) continue;
            var cluster = BinaryPrimitives.ReadUInt16LittleEndian(directory.AsSpan(offset + FatDirectoryLayout.FirstClusterOffset));
            var size = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + FatDirectoryLayout.FileSizeOffset));
            var isDirectory = attributes.HasFlag(FatDirectoryAttributes.Directory);
            IReadOnlyList<byte>? content = null;
            IReadOnlyList<FileSystemEntry> children = [];
            try
            {
                var bytes = ReadClusterChain(image, fat, layout, cluster, visited, warnings, name);
                if (isDirectory) children = ReadDirectory(image, bytes, fat, layout, warnings, depth + 1, CombinePath(path, name), new HashSet<int>());
                else content = bytes.Take(checked((int)Math.Min(size, int.MaxValue))).ToArray();
            }
            catch (InvalidDataException exception) { warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(name, exception)); }
            entries.Add(new(name, isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File, size, DecodeDateTime(BinaryPrimitives.ReadUInt16LittleEndian(directory.AsSpan(offset + FatDirectoryLayout.ModifiedDateOffset)), BinaryPrimitives.ReadUInt16LittleEndian(directory.AsSpan(offset + FatDirectoryLayout.ModifiedTimeOffset))), string.Empty, (uint)attributes, cluster, true, children, content));
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Lit une chaîne de clusters FAT12.</summary>
    private static byte[] ReadClusterChain(SectorImage image, byte[] fat, Fat12Layout layout, int first, HashSet<int> visited, List<string> warnings, string name)
    {
        if (first < Fat12Layout.FirstDataCluster) return [];
        using var stream = new MemoryStream();
        var cluster = first;
        while (cluster is >= Fat12Layout.FirstDataCluster and < Fat12Table.FirstEndOfChain)
        {
            if (cluster >= layout.ClusterCount + Fat12Layout.FirstDataCluster || !visited.Add(cluster)) throw Fat12FileSystemExceptions.InvalidChain(name, cluster);
            var firstSector = layout.DataStart + (cluster - Fat12Layout.FirstDataCluster) * layout.SectorsPerCluster;
            stream.Write(ReadSectors(image, firstSector, layout.SectorsPerCluster, warnings));
            if (!Fat12Table.TryRead(fat, cluster, out cluster)) throw Fat12FileSystemExceptions.TruncatedTable(cluster);
        }
        return stream.ToArray();
    }

    /// <summary>Lit une plage de secteurs en conservant la position des secteurs absents.</summary>
    private static byte[] ReadSectors(SectorImage image, int first, int count, List<string> warnings)
    {
        var output = new byte[count * FatBpbLayout.SectorSize];
        var missing = false;
        for (var index = 0; index < count; index++)
        {
            if (!image.TryGetBlock(first + index, out var block) || block.Data.Count != FatBpbLayout.SectorSize) { missing = true; continue; }
            block.Data.ToArray().CopyTo(output, index * FatBpbLayout.SectorSize);
        }
        if (missing) warnings.Add(Fat12FileSystemExceptions.MissingSectors(first, count));
        return output;
    }

    /// <summary>Lit la disposition BPB ou la disposition IBM historique d'un volume.</summary>
    private static bool TryReadLayout(ReadOnlySpan<byte> boot, int availableSectors, string formatId, out Fat12Layout layout)
    {
        layout = null!;
        var bytes = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.BytesPerSectorOffset..]);
        var cluster = boot[FatBpbLayout.SectorsPerClusterOffset];
        var reserved = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.ReservedSectorCountOffset..]);
        var fats = boot[FatBpbLayout.FatCountOffset];
        var roots = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.RootEntryCountOffset..]);
        var total = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.TotalSectors16Offset..]);
        if (total == 0) total = checked((ushort)Math.Min(ushort.MaxValue, BinaryPrimitives.ReadUInt32LittleEndian(boot[FatBpbLayout.TotalSectors32Offset..])));
        var fatSectors = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.SectorsPerFatOffset..]);
        if (bytes != FatBpbLayout.SectorSize || cluster == 0 || reserved == 0 || fats == 0 || roots == 0 || total == 0 || total > availableSectors || fatSectors == 0) return TryReadLegacyIbmLayout(formatId, availableSectors, boot, out layout);
        var rootSectors = (roots * FatDirectoryLayout.EntrySize + FatBpbLayout.SectorSize - 1) / FatBpbLayout.SectorSize;
        var rootStart = reserved + fats * fatSectors;
        var dataStart = rootStart + rootSectors;
        if (dataStart >= total) return false;
        var clusters = (total - dataStart) / cluster;
        if (clusters is <= 0 or >= Fat12Layout.MaximumClusterCount) return false;
        layout = new(reserved, fatSectors, rootStart, rootSectors, dataStart, cluster, clusters);
        return true;
    }

    /// <summary>Résout une disposition IBM historique sans BPB exploitable.</summary>
    private static bool TryReadLegacyIbmLayout(string formatId, int availableSectors, ReadOnlySpan<byte> boot, out Fat12Layout layout)
    {
        layout = null!;
        if (boot.Length == 0 || boot.IndexOfAnyExcept(boot[0]) < 0 || !IbmLegacyFat12Layout.TryResolve(formatId, out var parameters) || availableSectors < parameters.TotalSectors) return false;
        const int reserved = 1;
        const int fatCount = 2;
        var rootSectors = (parameters.RootEntries * FatDirectoryLayout.EntrySize + FatBpbLayout.SectorSize - 1) / FatBpbLayout.SectorSize;
        var rootStart = reserved + fatCount * parameters.SectorsPerFat;
        var dataStart = rootStart + rootSectors;
        var clusters = (parameters.TotalSectors - dataStart) / parameters.SectorsPerCluster;
        layout = new(reserved, parameters.SectorsPerFat, rootStart, rootSectors, dataStart, parameters.SectorsPerCluster, clusters);
        return true;
    }

    /// <summary>Vérifie l'en-tête de la première FAT.</summary>
    private static bool HasPlausibleFatHeader(SectorImage image, Fat12Layout layout) => image.TryGetBlock(layout.ReservedSectors, out var fat) && fat.Data.Count >= 3 && fat.Data[0] >= 0xf0 && fat.Data[1] == 0xff && fat.Data[2] == 0xff;

    /// <summary>Lit le label de volume présent dans le répertoire racine.</summary>
    private static string? ReadVolumeLabel(ReadOnlySpan<byte> root)
    {
        for (var offset = 0; offset + FatDirectoryLayout.EntrySize <= root.Length; offset += FatDirectoryLayout.EntrySize)
            if (root[offset] is not (FatDirectoryLayout.EndMarker or FatDirectoryLayout.DeletedMarker) && ((FatDirectoryAttributes)root[offset + FatDirectoryLayout.AttributesOffset]).HasFlag(FatDirectoryAttributes.VolumeLabel)) return ReadAscii(root, offset, FatDirectoryLayout.ExtensionOffset + FatDirectoryLayout.ExtensionLength).Trim();
        return null;
    }

    /// <summary>Décode un nom FAT au format 8.3.</summary>
    private static string DecodeName(ReadOnlySpan<byte> value)
    {
        var name = ReadAscii(value, 0, FatDirectoryLayout.NameLength).Trim();
        var extension = ReadAscii(value, FatDirectoryLayout.ExtensionOffset, FatDirectoryLayout.ExtensionLength).Trim();
        return extension.Length == 0 ? name : name + "." + extension;
    }

    /// <summary>Décode une chaîne Latin-1 de longueur fixe.</summary>
    private static string ReadAscii(ReadOnlySpan<byte> value, int offset, int length) => System.Text.Encoding.Latin1.GetString(value.Slice(offset, length)).TrimEnd(FatBpbLayout.NullPadding, FatBpbLayout.SpacePadding);

    /// <summary>Décode une date et une heure FAT.</summary>
    private static DateTimeOffset? DecodeDateTime(ushort date, ushort time)
    {
        try
        {
            var year = 1980 + (date >> 9);
            var month = date >> 5 & 15;
            var day = date & 31;
            if (month == 0 || day == 0) return null;
            return new DateTimeOffset(year, month, day, time >> 11, time >> 5 & 63, (time & 31) * 2, TimeSpan.Zero);
        }
        catch { return null; }
    }

    /// <summary>Concatène deux segments de chemin FAT.</summary>
    private static string CombinePath(string path, string name) => path.Length == 0 ? name : path + "/" + name;
}
