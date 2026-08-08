using System.Buffers.Binary;
using System.Text;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.FileSystems.Readers;

public sealed class ProDosFileSystemReader : IFileSystemReader
{
    public string Id => "prodos";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "apple2.prodos", "apple2.prodos.140", "apple2.prodos.800", "apple3.sos" };

    public bool CanRead(SectorImage image)
    {
        if (image.BlockSize != 512 || !image.TryGetBlock(2, out var root) || root.Data.Count != 512) return false;
        var header = root.Data[4]; return (header >> 4) == 0x0f && (header & 0x0f) is > 0 and <= 15 && root.Data[0x23] == 0x27;
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a ProDOS/SOS volume directory.");
        var root = image.GetBlock(2).Span; var name = ReadName(root, 4); var bitmap = ReadU16(root, 4 + 35); var total = ReadU16(root, 4 + 37);
        var warnings = new List<string>(); var entries = ReadDirectory(image, 2, warnings, new HashSet<int>(), 0);
        var free = CountFreeBlocks(image, bitmap, Math.Min(total, image.BlockCount), warnings);
        var system = image.FormatId.StartsWith("apple3.", StringComparison.OrdinalIgnoreCase) ? "Apple SOS / ProDOS" : "Apple ProDOS";
        return new(name, system, (long)Math.Min(total, image.BlockCount) * 512, (long)free * 512,
            ReadDate(root, 4 + 24), null, entries, warnings);
    }

    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, int firstBlock, List<string> warnings, HashSet<int> globalVisited, int depth)
    {
        if (depth > 64) { warnings.Add("The ProDOS directory nesting limit was reached."); return []; }
        var entries = new List<FileSystemEntry>(); var blockNumber = firstBlock; var chain = new HashSet<int>(); var first = true;
        while (blockNumber != 0)
        {
            if (!chain.Add(blockNumber) || !globalVisited.Add(blockNumber) || !image.TryGetBlock(blockNumber, out var block)) { warnings.Add($"Directory block {blockNumber} is missing or cyclic."); break; }
            var bytes = block.Data.ToArray(); var start = first ? 4 + 39 : 4;
            for (var offset = start; offset + 39 <= 512; offset += 39)
            {
                var storage = bytes[offset] >> 4; var nameLength = bytes[offset] & 0x0f;
                if (storage == 0 || nameLength == 0 || nameLength > 15) continue;
                var entryName = System.Text.Encoding.ASCII.GetString(bytes, offset + 1, nameLength); var key = ReadU16(bytes, offset + 17);
                var eof = bytes[offset + 21] | bytes[offset + 22] << 8 | bytes[offset + 23] << 16; var fileType = bytes[offset + 16];
                if (storage == 0x0d)
                {
                    var children = ReadDirectory(image, key, warnings, globalVisited, depth + 1);
                    entries.Add(new(entryName, FileSystemEntryKind.Directory, 0, ReadDate(bytes, offset + 33), TypeName(fileType), bytes[offset + 30], key, true, children));
                }
                else if (storage is >= 1 and <= 3)
                {
                    var content = ReadFile(image, storage, key, eof, warnings, entryName);
                    entries.Add(new(entryName, FileSystemEntryKind.File, eof, ReadDate(bytes, offset + 33), TypeName(fileType), bytes[offset + 30], key, true, [], content));
                }
            }
            blockNumber = ReadU16(bytes, 2); first = false;
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<byte> ReadFile(SectorImage image, int storage, int key, int length, List<string> warnings, string name)
    {
        var blocks = new List<int>();
        if (storage == 1) blocks.Add(key);
        else if (storage == 2) ReadIndex(image, key, blocks, warnings, name);
        else
        {
            if (!image.TryGetBlock(key, out var master)) warnings.Add($"{name}: master index block {key} is missing.");
            else for (var index = 0; index < 256; index++) { var child = Pointer(master.Data, index); if (child != 0) ReadIndex(image, child, blocks, warnings, name); }
        }
        using var output = new MemoryStream();
        foreach (var blockNumber in blocks)
        {
            if (!image.TryGetBlock(blockNumber, out var block)) { warnings.Add($"{name}: data block {blockNumber} is missing."); output.Write(new byte[512]); }
            else output.Write(block.Data.ToArray());
            if (output.Length >= length) break;
        }
        return output.ToArray().Take(length).ToArray();
    }

    private static void ReadIndex(SectorImage image, int blockNumber, List<int> output, List<string> warnings, string name)
    {
        if (!image.TryGetBlock(blockNumber, out var index)) { warnings.Add($"{name}: index block {blockNumber} is missing."); return; }
        for (var entry = 0; entry < 256; entry++) { var pointer = Pointer(index.Data, entry); if (pointer != 0) output.Add(pointer); }
    }

    private static int Pointer(IReadOnlyList<byte> block, int index) => block[index] | block[index + 256] << 8;
    private static int CountFreeBlocks(SectorImage image, int bitmapStart, int total, List<string> warnings)
    {
        var free = 0;
        for (var block = 0; block < total; block++)
        {
            var mapBlock = bitmapStart + block / 4096;
            if (!image.TryGetBlock(mapBlock, out var bitmap)) { if (block % 4096 == 0) warnings.Add($"Bitmap block {mapBlock} is missing."); continue; }
            var bit = block % 4096; if ((bitmap.Data[bit / 8] & (0x80 >> (bit & 7))) != 0) free++;
        }
        return free;
    }

    private static int ReadU16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, 2));
    private static string ReadName(ReadOnlySpan<byte> data, int offset) { var len = data[offset] & 0x0f; return System.Text.Encoding.ASCII.GetString(data.Slice(offset + 1, len)); }
    private static DateTimeOffset? ReadDate(ReadOnlySpan<byte> data, int offset) { if (offset + 4 > data.Length) return null; var date = ReadU16(data, offset); var time = ReadU16(data, offset + 2); try { var year = 1900 + (date >> 9); if (year < 1940) year += 100; return new DateTimeOffset(year, (date >> 5) & 15, date & 31, time >> 8, time & 0x3f, 0, TimeSpan.Zero); } catch { return null; } }
    private static string TypeName(byte type) => type switch { 0x04 => "Text", 0x06 => "Binary", 0x0f => "Directory", 0xfc => "BASIC", 0xfd => "Variables", 0xff => "System", _ => $"ProDOS ${type:X2}" };
}
