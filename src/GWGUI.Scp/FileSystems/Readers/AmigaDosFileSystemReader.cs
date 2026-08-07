using System.Buffers.Binary;
using System.Text;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.FileSystems.Readers;

public sealed class AmigaDosFileSystemReader : IFileSystemReader
{
    private const int BlockSize = 512;
    private const int HashTableEntries = 72;
    private static readonly DateTimeOffset AmigaEpoch = new(1978, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public string Id => "amigados";

    public bool CanRead(SectorImage image)
    {
        if (image.BlockSize != BlockSize || !image.TryGetBlock(0, out var boot) || boot.Data.Count < 4) return false;
        return boot.Data[0] == (byte)'D' && boot.Data[1] == (byte)'O' && boot.Data[2] == (byte)'S' && boot.Data[3] <= 7;
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a supported AmigaDOS boot block.");
        var warnings = new List<string>();
        var boot = image.GetBlock(0).Span;
        var dosType = boot[3];
        var rootPointer = ReadInt32(boot, 8);
        var rootBlock = rootPointer > 0 && rootPointer < image.BlockCount ? rootPointer : image.BlockCount / 2;
        var root = ReadRequiredBlock(image, rootBlock, "root block");
        if (ReadInt32(root, 0) != 2 || ReadInt32(root, 508) != 1) throw new InvalidDataException("The AmigaDOS root block is invalid.");
        if (!ChecksumValid(root)) warnings.Add($"Root block {rootBlock} has an invalid checksum.");
        var hashSize = Math.Clamp(ReadInt32(root, 12), 0, HashTableEntries);
        if (hashSize == 0) hashSize = HashTableEntries;
        var visited = new HashSet<int> { rootBlock };
        var entries = ReadDirectory(image, root, hashSize, dosType, visited, warnings, 0);
        var freeBlocks = CountFreeBlocks(image, root, warnings);
        var fileSystem = dosType switch
        {
            0 => "AmigaDOS OFS", 1 => "AmigaDOS FFS", 2 => "AmigaDOS OFS International",
            3 => "AmigaDOS FFS International", 4 => "AmigaDOS OFS Directory Cache",
            5 => "AmigaDOS FFS Directory Cache", 6 => "AmigaDOS OFS Long Names", 7 => "AmigaDOS FFS Long Names",
            _ => "AmigaDOS"
        };
        return new(ReadBString(root, 432, 30), fileSystem, image.Capacity, (long)freeBlocks * BlockSize,
            ReadDate(root, 420), ReadDate(root, 472), entries, warnings);
    }

    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, ReadOnlySpan<byte> directory, int hashSize, byte dosType,
        HashSet<int> visited, List<string> warnings, int depth)
    {
        if (depth > 64) { warnings.Add("The directory nesting limit was reached."); return []; }
        var entries = new List<FileSystemEntry>();
        for (var index = 0; index < hashSize; index++)
        {
            var blockNumber = ReadInt32(directory, 24 + index * 4);
            var chain = new HashSet<int>();
            while (blockNumber != 0)
            {
                if (blockNumber < 0 || blockNumber >= image.BlockCount || !chain.Add(blockNumber))
                {
                    warnings.Add($"Invalid or cyclic directory chain at block {blockNumber}.");
                    break;
                }
                if (!image.TryGetBlock(blockNumber, out var sector))
                {
                    warnings.Add($"Directory entry block {blockNumber} is missing.");
                    break;
                }
                var block = sector.Data.ToArray().AsSpan();
                var next = ReadInt32(block, 496);
                if (!visited.Add(blockNumber)) { blockNumber = next; continue; }
                var type = ReadInt32(block, 508);
                var name = ReadEntryName(block, dosType);
                var kind = type switch { 2 => FileSystemEntryKind.Directory, -3 => FileSystemEntryKind.File, 3 or 4 or -4 => FileSystemEntryKind.Link, _ => FileSystemEntryKind.Unknown };
                var children = kind == FileSystemEntryKind.Directory
                    ? ReadDirectory(image, block, HashTableEntries, dosType, visited, warnings, depth + 1)
                    : Array.Empty<FileSystemEntry>();
                IReadOnlyList<byte>? content = null;
                var size = kind == FileSystemEntryKind.File ? ReadUInt32(block, 324) : 0;
                if (kind == FileSystemEntryKind.File)
                {
                    try { content = ReadFile(image, block, checked((int)size), (dosType & 1) != 0, warnings); }
                    catch (Exception exception) when (exception is InvalidDataException or OverflowException) { warnings.Add($"{name}: {exception.Message}"); }
                }
                entries.Add(new(name, kind, size, ReadDate(block, 420), ReadBString(block, 328, 79), ReadUInt32(block, 320), blockNumber,
                    ChecksumValid(block), children, content));
                blockNumber = next;
            }
        }
        return entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<byte> ReadFile(SectorImage image, ReadOnlySpan<byte> header, int size, bool fastFileSystem, List<string> warnings)
    {
        var output = new List<byte>(size);
        var metadata = header.ToArray();
        var extensionVisited = new HashSet<int>();
        while (true)
        {
            var highSequence = Math.Clamp(ReadInt32(metadata, 8), 0, 72);
            for (var index = 0; index < highSequence && output.Count < size; index++)
            {
                var dataBlock = ReadInt32(metadata, 24 + (71 - index) * 4);
                if (dataBlock <= 0 || dataBlock >= image.BlockCount || !image.TryGetBlock(dataBlock, out var sector))
                {
                    warnings.Add($"File data block {dataBlock} is missing.");
                    continue;
                }
                var data = sector.Data.ToArray();
                if (fastFileSystem) output.AddRange(data.Take(Math.Min(data.Length, size - output.Count)));
                else
                {
                    var length = Math.Clamp(ReadInt32(data, 12), 0, 488);
                    if (ReadInt32(data, 0) != 8) warnings.Add($"OFS data block {dataBlock} has an unexpected type.");
                    output.AddRange(data.Skip(24).Take(Math.Min(length, size - output.Count)));
                }
            }
            var extension = ReadInt32(metadata, 504);
            if (extension == 0) break;
            if (extension < 0 || extension >= image.BlockCount || !extensionVisited.Add(extension) || !image.TryGetBlock(extension, out var extensionBlock))
                throw new InvalidDataException($"Invalid file extension block {extension}.");
            metadata = extensionBlock.Data.ToArray();
        }
        return output.Take(size).ToArray();
    }

    private static int CountFreeBlocks(SectorImage image, ReadOnlySpan<byte> root, List<string> warnings)
    {
        var count = 0;
        for (var pointer = 0; pointer < 25; pointer++)
        {
            var bitmapBlock = ReadInt32(root, 316 + pointer * 4);
            if (bitmapBlock == 0) break;
            if (!image.TryGetBlock(bitmapBlock, out var sector)) { warnings.Add($"Bitmap block {bitmapBlock} is missing."); continue; }
            var bitmap = sector.Data.ToArray().AsSpan();
            if (!ChecksumValid(bitmap)) warnings.Add($"Bitmap block {bitmapBlock} has an invalid checksum.");
            for (var offset = 4; offset < BlockSize; offset += 4) count += System.Numerics.BitOperations.PopCount(ReadUInt32(bitmap, offset));
        }
        return Math.Min(count, image.BlockCount);
    }

    private static string ReadEntryName(ReadOnlySpan<byte> block, byte dosType)
    {
        var ordinary = ReadBString(block, 432, 30);
        if (ordinary.Length > 0 || dosType < 6) return ordinary;
        return ReadBString(block, 328, 107);
    }

    private static string ReadBString(ReadOnlySpan<byte> block, int offset, int maximum)
    {
        if (offset < 0 || offset >= block.Length) return string.Empty;
        var length = Math.Min(block[offset], Math.Min(maximum, block.Length - offset - 1));
        return System.Text.Encoding.Latin1.GetString(block.Slice(offset + 1, length)).TrimEnd('\0');
    }

    private static DateTimeOffset? ReadDate(ReadOnlySpan<byte> block, int offset)
    {
        var days = ReadInt32(block, offset); var minutes = ReadInt32(block, offset + 4); var ticks = ReadInt32(block, offset + 8);
        if (days < 0 || minutes < 0 || minutes >= 24 * 60 || ticks < 0 || ticks >= 60 * 50) return null;
        try { return AmigaEpoch.AddDays(days).AddMinutes(minutes).AddMilliseconds(ticks * 20d); } catch { return null; }
    }

    private static ReadOnlySpan<byte> ReadRequiredBlock(SectorImage image, int blockNumber, string description)
    {
        if (!image.TryGetBlock(blockNumber, out var block)) throw new InvalidDataException($"The AmigaDOS {description} ({blockNumber}) is missing.");
        return block.Data.ToArray();
    }

    private static bool ChecksumValid(ReadOnlySpan<byte> block)
    {
        if (block.Length != BlockSize) return false;
        uint sum = 0; for (var offset = 0; offset < block.Length; offset += 4) sum = unchecked(sum + ReadUInt32(block, offset));
        return sum == 0;
    }

    private static int ReadInt32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32BigEndian(data.Slice(offset, 4));
    private static uint ReadUInt32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));
}
