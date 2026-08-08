using System.Buffers.Binary;
using System.Text;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.FileSystems.Readers;

/// <summary>
/// Reads tagged Lisa Office System media. Lisa pages identify their owner through
/// the 12-byte page tag, which also makes a damaged catalog partially recoverable.
/// </summary>
public sealed class LisaFileSystemReader : IFileSystemReader
{
    private const ushort MddfFileId = 0x0001;
    private const ushort BitmapFileId = 0x0002;
    private const ushort SRecordsFileId = 0x0003;
    private const ushort CatalogFileId = 0x0004;

    public string Id => "lisa";
    public IReadOnlySet<string> CatalogFormatIds { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "applelisa.office", "mac.400" };

    public bool CanRead(SectorImage image) =>
        image.FormatId.Equals("applelisa.office", StringComparison.OrdinalIgnoreCase) &&
        image.AvailableBlocks.Any(block => TagFileId(block) == MddfFileId);

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image))
            throw new InvalidDataException("The image does not contain a tagged Lisa file system.");

        var warnings = new List<string>();
        var mddfBlocks = image.AvailableBlocks.Where(block => TagFileId(block) == MddfFileId).ToArray();
        var mddfBytes = mddfBlocks[^1].Data.ToArray();
        var mddf = mddfBytes.AsSpan();
        var version = BinaryPrimitives.ReadUInt16BigEndian(mddf);
        var volumeNameLength = mddf.Length > 12 ? Math.Min(mddf[12], (byte)31) : 0;
        var volumeName = volumeNameLength > 0 && mddf.Length >= 13 + volumeNameLength
            ? ReadLisaString(mddf.Slice(13, volumeNameLength))
            : string.Empty;
        if (string.IsNullOrWhiteSpace(volumeName)) volumeName = "Lisa";

        var names = ReadCatalogNames(image, warnings);
        var entries = new List<FileSystemEntry>();
        foreach (var group in image.AvailableBlocks
                     .Select(block => (Block: block, FileId: TagFileId(block)))
                     .Where(item => IsUserFile(item.FileId))
                     .GroupBy(item => item.FileId)
                     .OrderBy(group => group.Key))
        {
            var ordered = group.OrderBy(item => TagPageNumber(item.Block)).ToArray();
            using var content = new MemoryStream();
            foreach (var item in ordered) content.Write(item.Block.Data.ToArray());
            var name = names.TryGetValue(group.Key, out var catalogName)
                ? catalogName
                : $"File {group.Key:X4}";
            entries.Add(new(name, FileSystemEntryKind.File, content.Length, null,
                $"Lisa file ${group.Key:X4}", 0, ordered[0].Block.LogicalBlock,
                names.ContainsKey(group.Key), [], content.ToArray()));
        }

        var freePages = image.AvailableBlocks.Count(block => TagFileId(block) is 0x0000 or 0x7fff);
        var fileSystemName = version switch
        {
            0x000e => "Lisa Office System (table catalog)",
            0x000f => "Lisa Office System (hash catalog)",
            0x0011 => "Lisa Office System (B-tree catalog)",
            _ => $"Lisa Office System (${version:X4})"
        };
        return new(volumeName, fileSystemName, image.Capacity, (long)freePages * image.BlockSize,
            null, null, entries, warnings);
    }

    private static Dictionary<ushort, string> ReadCatalogNames(SectorImage image, List<string> warnings)
    {
        var result = new Dictionary<ushort, string>();
        var catalogPages = image.AvailableBlocks
            .Where(block => TagFileId(block) == CatalogFileId)
            .OrderBy(TagPageNumber)
            .ToArray();
        if (catalogPages.Length == 0)
        {
            warnings.Add("The Lisa catalog pages are missing; file names were recovered from page tags only.");
            return result;
        }

        var bytes = catalogPages.SelectMany(block => block.Data).ToArray();
        // Lisa catalog entries occupy 64 bytes. The first name begins at offset 0x51;
        // its file identifier follows at entry offset + 36 (big endian).
        for (var offset = 0x50; offset + 64 <= bytes.Length; offset += 64)
        {
            if (bytes[offset] != 0 || bytes[offset + 1] < 0x20) continue;
            var name = ReadLisaString(bytes.AsSpan(offset + 1, 31));
            var fileId = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 36, 2));
            if (!IsUserFile(fileId) || string.IsNullOrWhiteSpace(name)) continue;
            result.TryAdd(fileId, name);
        }
        return result;
    }

    private static ushort TagFileId(SectorBlock block)
    {
        if (block.Tag is null || block.Tag.Count < 6) return 0;
        return (ushort)((block.Tag[4] << 8) | block.Tag[5]);
    }

    private static int TagPageNumber(SectorBlock block)
    {
        if (block.Tag is null || block.Tag.Count < 8) return block.LogicalBlock;
        return ((block.Tag[6] << 8) | block.Tag[7]) & 0x07ff;
    }

    private static bool IsUserFile(ushort fileId) =>
        fileId is > CatalogFileId and < 0x7fff &&
        fileId is not 0x00aa and not 0x00bb and not 0xaaaa and not 0xbbbb;

    private static string ReadLisaString(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.IndexOf((byte)0);
        if (end >= 0) bytes = bytes[..end];
        return System.Text.Encoding.Latin1.GetString(bytes).Trim(' ', '\0');
    }
}
