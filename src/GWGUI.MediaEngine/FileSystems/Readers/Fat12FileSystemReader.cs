using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class Fat12FileSystemReader : IFileSystemReader
{
    public string Id => Definitions.FileSystemIds.Fat12;
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        DiskImageFormatIds.AtariSt180, DiskImageFormatIds.AtariSt360, DiskImageFormatIds.AtariSt400,
        DiskImageFormatIds.AtariSt440, DiskImageFormatIds.AtariSt720, DiskImageFormatIds.AtariSt800,
        DiskImageFormatIds.AtariSt810, DiskImageFormatIds.AtariSt880, DiskImageFormatIds.AtariSt1440,
        DiskImageFormatIds.Ibm160, DiskImageFormatIds.Ibm180, DiskImageFormatIds.Ibm320,
        DiskImageFormatIds.Ibm360, DiskImageFormatIds.Ibm720, DiskImageFormatIds.Ibm800,
        DiskImageFormatIds.Ibm1200, DiskImageFormatIds.Ibm1440, DiskImageFormatIds.Ibm1680,
        DiskImageFormatIds.IbmDmf, DiskImageFormatIds.Ibm2880, DiskImageFormatIds.IbmScan,
        DiskImageFormatIds.Msx1D, DiskImageFormatIds.Msx1Dd, DiskImageFormatIds.Msx2D, DiskImageFormatIds.Msx2Dd
    };

    public bool CanRead(SectorImage image)
    {
        if (image.BlockSize != 512 || !image.TryGetBlock(0, out var boot) || boot.Data.Count < 64) return false;
        return TryReadLayout(boot.Data.ToArray(), image.BlockCount, image.FormatId, out var layout)
            && HasPlausibleFatHeader(image, layout);
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!image.TryGetBlock(0, out var boot) || !TryReadLayout(boot.Data.ToArray(), image.BlockCount, image.FormatId, out var layout)
            || !HasPlausibleFatHeader(image, layout))
            throw new InvalidDataException("The image does not contain a supported FAT12 file system.");
        var warnings = new List<string>(); var fat = ReadSectors(image, layout.ReservedSectors, layout.SectorsPerFat, warnings);
        var root = ReadSectors(image, layout.RootStart, layout.RootSectors, warnings);
        var entries = ReadDirectory(image, root, fat, layout, warnings, 0, new HashSet<int>());
        var freeClusters = Enumerable.Range(2, Math.Max(0, layout.ClusterCount - 2)).Count(cluster => ReadFat12(fat, cluster) == 0);
        var label = ReadVolumeLabel(root) ?? ReadBootVolumeLabel(boot.Data);
        var fileSystemName = Definitions.FileSystemDisplayNames.Fat12(image.FormatId);
        return new(label, fileSystemName, image.Capacity, (long)freeClusters * layout.SectorsPerCluster * 512,
            null, null, entries, warnings);
    }

    private static string ReadBootVolumeLabel(IReadOnlyList<byte> boot)
    {
        if (boot.Count < 54) return string.Empty;
        var bytes = boot.Skip(43).Take(11).ToArray();
        if (bytes.Any(value => value is < 0x20 or > 0x7e)) return string.Empty;
        var label = System.Text.Encoding.ASCII.GetString(bytes).Trim();
        return label.Equals("NO NAME", StringComparison.OrdinalIgnoreCase) ? string.Empty : label;
    }

    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, byte[] directory, byte[] fat, Layout layout,
        List<string> warnings, int depth, HashSet<int> visited)
    {
        if (depth > 64) { warnings.Add("The FAT directory nesting limit was reached."); return []; }
        var entries = new List<FileSystemEntry>();
        for (var offset = 0; offset + 32 <= directory.Length; offset += 32)
        {
            var first = directory[offset]; if (first == 0) break; if (first == 0xe5) continue;
            var attributes = directory[offset + 11]; if ((attributes & 0x0f) == 0x0f || (attributes & 0x08) != 0) continue;
            var name = DecodeName(directory.AsSpan(offset, 11)); if (name is "." or ".." || name.Length == 0) continue;
            var cluster = BinaryPrimitives.ReadUInt16LittleEndian(directory.AsSpan(offset + 26));
            var size = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + 28));
            var isDirectory = (attributes & 0x10) != 0; IReadOnlyList<byte>? content = null; IReadOnlyList<FileSystemEntry> children = [];
            try
            {
                var bytes = ReadClusterChain(image, fat, layout, cluster, visited, warnings);
                if (isDirectory) children = ReadDirectory(image, bytes, fat, layout, warnings, depth + 1, new HashSet<int>());
                else content = bytes.Take(checked((int)Math.Min(size, int.MaxValue))).ToArray();
            }
            catch (InvalidDataException exception) { warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(name, exception)); }
            entries.Add(new(name, isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File, size,
                DecodeDateTime(BinaryPrimitives.ReadUInt16LittleEndian(directory.AsSpan(offset + 24)), BinaryPrimitives.ReadUInt16LittleEndian(directory.AsSpan(offset + 22))),
                string.Empty, attributes, cluster, true, children, content));
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static byte[] ReadClusterChain(SectorImage image, byte[] fat, Layout layout, int first, HashSet<int> visited, List<string> warnings)
    {
        if (first < 2) return [];
        using var stream = new MemoryStream(); var cluster = first;
        while (cluster is >= 2 and < 0xff8)
        {
            if (cluster >= layout.ClusterCount + 2 || !visited.Add(cluster)) throw new InvalidDataException($"Invalid or cyclic FAT chain at cluster {cluster}.");
            var firstSector = layout.DataStart + (cluster - 2) * layout.SectorsPerCluster;
            var bytes = ReadSectors(image, firstSector, layout.SectorsPerCluster, warnings); stream.Write(bytes);
            cluster = ReadFat12(fat, cluster);
        }
        return stream.ToArray();
    }

    private static byte[] ReadSectors(SectorImage image, int first, int count, List<string> warnings)
    {
        var output = new byte[count * 512];
        for (var index = 0; index < count; index++)
        {
            if (!image.TryGetBlock(first + index, out var block) || block.Data.Count != 512) { warnings.Add($"Sector {first + index} is missing."); continue; }
            block.Data.ToArray().CopyTo(output, index * 512);
        }
        return output;
    }

    private static bool TryReadLayout(ReadOnlySpan<byte> boot, int availableSectors, string formatId, out Layout layout)
    {
        layout = default; var bytes = BinaryPrimitives.ReadUInt16LittleEndian(boot[11..]); var cluster = boot[13];
        var reserved = BinaryPrimitives.ReadUInt16LittleEndian(boot[14..]); var fats = boot[16];
        var roots = BinaryPrimitives.ReadUInt16LittleEndian(boot[17..]); var total = BinaryPrimitives.ReadUInt16LittleEndian(boot[19..]);
        if (total == 0) total = checked((ushort)Math.Min(ushort.MaxValue, BinaryPrimitives.ReadUInt32LittleEndian(boot[32..])));
        var fatSectors = BinaryPrimitives.ReadUInt16LittleEndian(boot[22..]);
        if (bytes != 512 || cluster == 0 || reserved == 0 || fats == 0 || roots == 0 || total == 0 || total > availableSectors || fatSectors == 0)
            return TryReadLegacyIbmLayout(formatId, availableSectors, boot, out layout);
        var rootSectors = (roots * 32 + 511) / 512; var rootStart = reserved + fats * fatSectors; var dataStart = rootStart + rootSectors;
        if (dataStart >= total) return false; var clusters = (total - dataStart) / cluster;
        if (clusters >= 4085) return false; layout = new(reserved, fatSectors, rootStart, rootSectors, dataStart, cluster, clusters); return true;
    }

    private static bool TryReadLegacyIbmLayout(string formatId, int availableSectors, ReadOnlySpan<byte> boot, out Layout layout)
    {
        layout = default;
        if (boot.Length == 0 || boot.IndexOfAnyExcept(boot[0]) < 0) return false;
        var parameters = formatId.ToLowerInvariant() switch
        {
            DiskImageFormatIds.Ibm160 => (Total: 320, SectorsPerCluster: 1, RootEntries: 64, SectorsPerFat: 1),
            DiskImageFormatIds.Ibm180 => (Total: 360, SectorsPerCluster: 1, RootEntries: 64, SectorsPerFat: 2),
            DiskImageFormatIds.Ibm320 => (Total: 640, SectorsPerCluster: 2, RootEntries: 112, SectorsPerFat: 1),
            DiskImageFormatIds.Ibm360 => (Total: 720, SectorsPerCluster: 2, RootEntries: 112, SectorsPerFat: 2),
            _ => default
        };
        if (parameters.Total == 0 || availableSectors < parameters.Total) return false;
        const int reserved = 1, fats = 2;
        var rootSectors = (parameters.RootEntries * 32 + 511) / 512;
        var rootStart = reserved + fats * parameters.SectorsPerFat;
        var dataStart = rootStart + rootSectors;
        var clusters = (parameters.Total - dataStart) / parameters.SectorsPerCluster;
        layout = new(reserved, parameters.SectorsPerFat, rootStart, rootSectors, dataStart, parameters.SectorsPerCluster, clusters);
        return true;
    }

    private static bool HasPlausibleFatHeader(SectorImage image, Layout layout)
    {
        if (!image.TryGetBlock(layout.ReservedSectors, out var fat) || fat.Data.Count < 3) return false;
        return fat.Data[0] >= 0xf0 && fat.Data[1] == 0xff && fat.Data[2] == 0xff;
    }

    private static int ReadFat12(ReadOnlySpan<byte> fat, int cluster)
    {
        var offset = cluster + cluster / 2; if (offset + 1 >= fat.Length) return 0xfff;
        var pair = fat[offset] | fat[offset + 1] << BitPrimitives.BitsPerByte; return (cluster & 1) == 0 ? pair & 0xfff : pair >> 4;
    }
    private static string? ReadVolumeLabel(ReadOnlySpan<byte> root) { for (var i = 0; i + 32 <= root.Length; i += 32) if (root[i] is not (0 or 0xe5) && (root[i + 11] & 0x08) != 0) return ReadAscii(root, i, 11).Trim(); return null; }
    private static string DecodeName(ReadOnlySpan<byte> value) { var name = ReadAscii(value, 0, 8).Trim(); var ext = ReadAscii(value, 8, 3).Trim(); return ext.Length == 0 ? name : name + "." + ext; }
    private static string ReadAscii(IReadOnlyList<byte> value, int offset, int length) => System.Text.Encoding.Latin1.GetString(value.Skip(offset).Take(length).ToArray()).TrimEnd('\0', ' ');
    private static string ReadAscii(ReadOnlySpan<byte> value, int offset, int length) => System.Text.Encoding.Latin1.GetString(value.Slice(offset, length)).TrimEnd('\0', ' ');
    private static DateTimeOffset? DecodeDateTime(ushort date, ushort time) { try { var year = 1980 + (date >> 9); var month = date >> 5 & 15; var day = date & 31; if (month == 0 || day == 0) return null; return new DateTimeOffset(year, month, day, time >> 11, time >> 5 & 63, (time & 31) * 2, TimeSpan.Zero); } catch { return null; } }
    private readonly record struct Layout(int ReservedSectors, int SectorsPerFat, int RootStart, int RootSectors, int DataStart, int SectorsPerCluster, int ClusterCount);
}
