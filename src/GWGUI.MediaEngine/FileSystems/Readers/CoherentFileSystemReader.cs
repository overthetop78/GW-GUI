using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.FileSystems.Coherent;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Read-only reader for the V7-style COHERENT file system used by the Commodore 900.</summary>
public sealed class CoherentFileSystemReader : IFileSystemReader
{
    private const int BlockSize = 512;
    private const int InodeSize = 64;
    private const ushort DirectoryMode = 0x4000;
    private const ushort TypeMask = 0xf000;

    public string Id => "coherent";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.Commodore900Coherent };

    public bool CanRead(SectorImage image) => CatalogFormatIds.Contains(image.FormatId) && image.BlockSize == BlockSize;

    public FileSystemVolume Read(SectorImage image)
    {
        var bytes = Flatten(image);
        if (!CoherentSuperblockProbe.LooksLikeCoherent(bytes))
            throw new InvalidDataException("The COHERENT superblock is missing.");
        var inodeZoneEnd = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(512, 2));
        var fileSystemBlocks = checked((int)CoherentSuperblockProbe.ReadCanonicalUInt32(bytes.AsSpan(514, 4)));
        if (inodeZoneEnd < 3 || inodeZoneEnd > fileSystemBlocks) throw new InvalidDataException("The COHERENT inode zone is invalid.");

        var warnings = new List<string>();
        var visited = new HashSet<ushort>();
        var entries = ReadDirectory(bytes, 2, visited, warnings);
        var volumeName = DecodeFixed(bytes.AsSpan(996, 6));
        if (volumeName is "xxxxx" or "noname") volumeName = string.Empty;
        var freeBytes = (long)CoherentSuperblockProbe.ReadCanonicalUInt32(bytes.AsSpan(980, 4)) * BlockSize;
        var modified = DecodeTime(CoherentSuperblockProbe.ReadCanonicalUInt32(bytes.AsSpan(976, 4)));
        return new(volumeName, "COHERENT (Commodore 900)", (long)fileSystemBlocks * BlockSize,
            Math.Clamp(freeBytes, 0, (long)fileSystemBlocks * BlockSize), null, modified, entries, warnings);
    }

    private static IReadOnlyList<FileSystemEntry> ReadDirectory(byte[] image, ushort inodeNumber, HashSet<ushort> visited, List<string> warnings)
    {
        if (!visited.Add(inodeNumber)) return [];
        var inode = ReadInode(image, inodeNumber);
        var data = ReadFileData(image, inode, warnings, $"inode {inodeNumber}");
        var result = new List<FileSystemEntry>();
        for (var offset = 0; offset + 16 <= data.Length; offset += 16)
        {
            var childNumber = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));
            if (childNumber == 0) continue;
            var name = DecodeFixed(data.AsSpan(offset + 2, 14));
            if (name.Length == 0 || name is "." or "..") continue;
            try
            {
                var child = ReadInode(image, childNumber);
                var directory = (child.Mode & TypeMask) == DirectoryMode;
                var content = directory ? null : ReadFileData(image, child, warnings, name);
                var children = directory ? ReadDirectory(image, childNumber, visited, warnings) : [];
                result.Add(new(name, directory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File,
                    child.Size, DecodeTime(child.Modified), $"COHERENT inode {childNumber}", (uint)(child.Mode & 0x0fff),
                    childNumber, true, children, content));
            }
            catch (InvalidDataException exception)
            {
                warnings.Add($"{name}: {exception.Message}");
            }
        }
        return result.OrderByDescending(entry => entry.Kind == FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static Inode ReadInode(byte[] image, ushort number)
    {
        if (number == 0) throw new InvalidDataException("Invalid COHERENT inode 0.");
        var offset = BlockSize * 2 + (number - 1) * InodeSize;
        if (offset < 0 || offset + InodeSize > image.Length) throw new InvalidDataException($"COHERENT inode {number} is outside the image.");
        var value = image.AsSpan(offset, InodeSize);
        var pointers = new int[13];
        for (var index = 0; index < pointers.Length; index++)
        {
            var item = value.Slice(12 + index * 3, 3);
            pointers[index] = item[1] | item[2] << 8 | item[0] << 16;
        }
        return new(BinaryPrimitives.ReadUInt16LittleEndian(value), CoherentSuperblockProbe.ReadCanonicalUInt32(value.Slice(8, 4)),
            pointers, CoherentSuperblockProbe.ReadCanonicalUInt32(value.Slice(56, 4)));
    }

    private static byte[] ReadFileData(byte[] image, Inode inode, List<string> warnings, string name)
    {
        if (inode.Size > int.MaxValue) throw new InvalidDataException("The file is too large.");
        // Device nodes store the device identifier in i_data rather than file-system
        // block addresses. They normally have a zero length and must not be walked as
        // regular files (otherwise every encoded device number looks like a bad block).
        if (inode.Size == 0) return [];
        var requiredBlocks = checked(((int)inode.Size + BlockSize - 1) / BlockSize);
        var blocks = new List<int>(requiredBlocks);
        for (var index = 0; index < 10 && blocks.Count < requiredBlocks; index++) blocks.Add(inode.Blocks[index]);
        AddIndirect(image, inode.Blocks[10], 1, blocks, requiredBlocks, warnings, name);
        AddIndirect(image, inode.Blocks[11], 2, blocks, requiredBlocks, warnings, name);
        AddIndirect(image, inode.Blocks[12], 3, blocks, requiredBlocks, warnings, name);
        var result = new byte[checked((int)inode.Size)];
        var destination = 0;
        foreach (var block in blocks)
        {
            if (destination >= result.Length) break;
            var count = Math.Min(BlockSize, result.Length - destination);
            if (block == 0) { destination += count; continue; }
            var source = block * BlockSize;
            if (block <= 0 || source + BlockSize > image.Length) { warnings.Add($"{name}: COHERENT block {block} is outside the image."); continue; }
            image.AsSpan(source, count).CopyTo(result.AsSpan(destination));
            destination += count;
        }
        if (destination < result.Length) warnings.Add($"{name}: {result.Length - destination} byte(s) could not be read.");
        return result;
    }

    private static void AddIndirect(byte[] image, int block, int depth, List<int> result, int requiredBlocks, List<string> warnings, string name)
    {
        if (result.Count >= requiredBlocks) return;
        if (block == 0)
        {
            var capacity = 1;
            for (var index = 0; index < depth; index++) capacity *= BlockSize / 4;
            while (capacity-- > 0 && result.Count < requiredBlocks) result.Add(0);
            return;
        }
        var longOffset = (long)block * BlockSize;
        if (block <= 0 || longOffset < 0 || longOffset + BlockSize > image.Length) { warnings.Add($"{name}: indirect COHERENT block {block} is outside the image."); return; }
        var offset = (int)longOffset;
        for (var index = 0; index < BlockSize && result.Count < requiredBlocks; index += 4)
        {
            var rawChild = CoherentSuperblockProbe.ReadCanonicalUInt32(image.AsSpan(offset + index, 4));
            if (rawChild > image.Length / BlockSize) { warnings.Add($"{name}: indirect COHERENT block {rawChild} is outside the image."); continue; }
            var child = (int)rawChild;
            if (depth == 1) result.Add(child); else AddIndirect(image, child, depth - 1, result, requiredBlocks, warnings, name);
        }
    }

    private static string DecodeFixed(ReadOnlySpan<byte> bytes) => System.Text.Encoding.ASCII.GetString(bytes).TrimEnd('\0', ' ', '\n', '\r');
    private static DateTimeOffset? DecodeTime(uint seconds)
    {
        try { return seconds == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(seconds); }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    private static byte[] Flatten(SectorImage image)
    {
        var bytes = new byte[checked(image.BlockCount * image.BlockSize)];
        for (var block = 0; block < image.BlockCount; block++)
            if (image.TryGetBlock(block, out var sector) && sector.Data.Count == image.BlockSize)
                sector.Data.ToArray().CopyTo(bytes, block * image.BlockSize);
        return bytes;
    }

    private sealed record Inode(ushort Mode, uint Size, IReadOnlyList<int> Blocks, uint Modified);
}
