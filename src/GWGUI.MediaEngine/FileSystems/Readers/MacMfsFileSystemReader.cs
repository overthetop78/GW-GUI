using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.FileSystems.Macintosh;

namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class MacMfsFileSystemReader : IFileSystemReader
{
    private static readonly DateTimeOffset MacEpoch = new(1904, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public string Id => Definitions.FileSystemIds.MacMfs;
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AppleMacMfs, DiskImageFormatIds.Mac400, DiskImageFormatIds.Mac800, DiskImageFormatIds.Mac1440 };

    public bool CanRead(SectorImage image) => image.BlockSize == MacintoshVolumeSignatures.BlockSize && image.TryGetBlock(MacintoshVolumeSignatures.MasterDirectoryBlock, out var mdb) && mdb.Data.Count >= 64 && BinaryPrimitives.ReadUInt16BigEndian(mdb.Data.ToArray()) == MacintoshVolumeSignatures.Mfs;

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a Macintosh MFS volume.");
        var warnings = new List<string>();
        var mdb = ReadBlocks(image, 2, 2, warnings, "MFS volume information"); var directoryStart = U16(mdb, 14); var directoryLength = U16(mdb, 16);
        var allocationCount = U16(mdb, 18); var allocationSize = U32(mdb, 20); var allocationStart = U16(mdb, 28); var free = U16(mdb, 34);
        var name = Pascal(mdb, 36, 27); var map = DecodeAllocationMap(mdb.AsSpan(64, 960), allocationCount);
        var entries = new List<FileSystemEntry>();
        for (var blockNumber = directoryStart; blockNumber < directoryStart + directoryLength && blockNumber < image.BlockCount; blockNumber++)
        {
            if (!image.TryGetBlock(blockNumber, out var block)) { warnings.Add($"Directory block {blockNumber} is missing."); continue; }
            var bytes = block.Data.ToArray(); var offset = 0;
            while (offset + 51 <= bytes.Length && (bytes[offset] & 0x80) != 0)
            {
                var start = offset; var flags = bytes[offset++]; offset++; var finder = bytes.AsSpan(offset, 16).ToArray(); offset += 16;
                var fileNumber = U32(bytes, offset); offset += 4; var dataStart = U16(bytes, offset); offset += 2;
                var dataLogical = U32(bytes, offset); offset += 4; offset += 4; var resourceStart = U16(bytes, offset); offset += 2;
                var resourceLogical = U32(bytes, offset); offset += 4; offset += 4; var created = U32(bytes, offset); offset += 4; var modified = U32(bytes, offset); offset += 4;
                var nameLength = bytes[offset++]; if (nameLength > 63 || offset + nameLength > bytes.Length) { warnings.Add($"Invalid MFS directory entry in block {blockNumber}."); break; }
                var fileName = DecodeMac(bytes.AsSpan(offset, nameLength)); offset += nameLength; if ((offset & 1) != 0) offset++;
                var dataFork = ReadFork(image, map, allocationStart, allocationSize, dataStart, dataLogical, warnings, $"{fileName} (data fork)");
                var resourceFork = ReadFork(image, map, allocationStart, allocationSize, resourceStart, resourceLogical, warnings, $"{fileName} (resource fork)");
                // Classic Macintosh applications commonly keep nearly all their
                // useful bytes in the resource fork. Report the complete logical
                // file size and expose that fork when the data fork is empty.
                var content = dataFork.Count > 0 ? dataFork : resourceFork;
                var type = System.Text.Encoding.ASCII.GetString(finder, 0, 4).Trim('\0', ' ');
                var comment = string.IsNullOrWhiteSpace(type) ? "Macintosh file" : type;
                entries.Add(new(fileName, FileSystemEntryKind.File, (long)dataLogical + resourceLogical, MacDate(modified), comment, flags, (int)fileNumber, true, [], content));
                if (resourceLogical > 0 && resourceStart == 0) warnings.Add($"{fileName}: resource fork metadata is inconsistent.");
                if (offset <= start) break;
            }
        }
        return new(name, Definitions.FileSystemIds.MacMfs, image.Capacity, (long)free * allocationSize, MacDate(U32(mdb, 2)), MacDate(U32(mdb, 6)),
            entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static IReadOnlyList<byte> ReadFork(SectorImage image, ushort[] map, int allocationStart, uint allocationSize, int first, uint length, List<string> warnings, string name)
    {
        if (first == 0 || length == 0) return [];
        using var output = new MemoryStream(); var current = first; var visited = new HashSet<int>(); var logicalPerAllocation = checked((int)(allocationSize / 512));
        while (current >= 2 && current < 0xff1 && output.Length < length)
        {
            if (!visited.Add(current) || current - 2 >= map.Length) { warnings.Add($"{name}: invalid MFS allocation chain."); break; }
            var firstBlock = allocationStart + (current - 2) * logicalPerAllocation;
            for (var index = 0; index < logicalPerAllocation; index++)
            {
                if (!image.TryGetBlock(firstBlock + index, out var block)) { warnings.Add($"{name}: block {firstBlock + index} is missing."); output.Write(new byte[512]); }
                else output.Write(block.Data.ToArray());
            }
            current = map[current - 2];
        }
        return output.ToArray().Take(checked((int)Math.Min(length, int.MaxValue))).ToArray();
    }

    private static ushort[] DecodeAllocationMap(ReadOnlySpan<byte> packed, int count)
    {
        var result = new ushort[Math.Min(count, 640)];
        for (var index = 0; index + 1 < result.Length; index += 2)
        {
            var offset = index / 2 * 3; result[index] = (ushort)(packed[offset] << 4 | packed[offset + 1] >> 4);
            result[index + 1] = (ushort)((packed[offset + 1] & 15) << BitPrimitives.BitsPerByte | packed[offset + 2]);
        }
        return result;
    }

    private static byte[] ReadBlocks(SectorImage image, int first, int count, List<string> warnings, string description)
    {
        var result = new byte[count * 512];
        for (var index = 0; index < count; index++)
        {
            if (!image.TryGetBlock(first + index, out var block))
            {
                warnings.Add($"{description}: block {first + index} is missing.");
                continue;
            }
            block.Data.Take(512).ToArray().CopyTo(result, index * 512);
        }
        return result;
    }
    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset, 2));
    private static uint U32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));
    private static string Pascal(ReadOnlySpan<byte> data, int offset, int max) { var length = Math.Min(data[offset], max); return DecodeMac(data.Slice(offset + 1, length)); }
    private static string DecodeMac(ReadOnlySpan<byte> value) => System.Text.Encoding.Latin1.GetString(value).Replace(':', '/');
    private static DateTimeOffset? MacDate(uint seconds) { try { return seconds == 0 ? null : MacEpoch.AddSeconds(seconds); } catch { return null; } }
}
