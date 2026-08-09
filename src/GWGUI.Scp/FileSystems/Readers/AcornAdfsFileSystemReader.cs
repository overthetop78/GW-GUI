using System.Buffers.Binary;
using System.Text;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.FileSystems.Readers;

public sealed class AcornAdfsFileSystemReader : IFileSystemReader
{
    private const int BlockSize = 1024;
    private const int FileCoreUnitSize = 256;
    private const int DirectorySize = 2048;
    private const int EntryCount = 77;
    private const int EntrySize = 26;
    private static readonly DateTimeOffset RiscOsEpoch = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public string Id => "acorn-adfs";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "acorn.adfs.800" };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != BlockSize || image.BlockCount != 800)
            return false;
        var layout = CreateLayout(image);
        return TryReadDirectory(image, layout.RootAddress, layout.Resolve, out _);
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a supported Acorn ADFS catalogue.");
        var warnings = new List<string>();
        var layout = CreateLayout(image);
        var visited = new HashSet<int>();
        var root = ReadDirectory(image, layout.RootAddress, layout.Resolve, visited, warnings, 0);
        var volumeName = layout.VolumeName;
        var freeBytes = layout.FreeBytes;
        return new(volumeName.Length == 0 ? root.Name : volumeName, "Acorn ADFS", image.Capacity, freeBytes,
            null, null, root.Children, warnings);
    }

    private sealed record DirectoryData(string Name, string Title, IReadOnlyList<FileSystemEntry> Children);

    private static DirectoryData ReadDirectory(SectorImage image, int startBlock, AddressResolver resolve,
        HashSet<int> visited,
        List<string> warnings, int depth)
    {
        if (depth > 64)
        {
            warnings.Add("The ADFS directory nesting limit was reached.");
            return new("", "", []);
        }
        if (!visited.Add(startBlock))
        {
            warnings.Add($"The ADFS directory at sector {startBlock} is cyclic or referenced more than once.");
            return new("", "", []);
        }
        if (!TryReadDirectory(image, startBlock, resolve, out var directory))
            throw new InvalidDataException($"The ADFS directory at sector {startBlock} is invalid or incomplete.");

        var entries = new List<FileSystemEntry>();
        for (var index = 0; index < EntryCount; index++)
        {
            var offset = 5 + index * EntrySize;
            if (directory[offset] == 0) break;
            var name = DecodeName(directory.AsSpan(offset, 10));
            if (name.Length == 0) continue;
            var load = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + 10, 4));
            var execute = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + 14, 4));
            var length = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + 18, 4));
            var indirectAddress = ReadUInt24(directory, offset + 22);
            var attributes = directory[offset + 25];
            var isDirectory = (attributes & 0x08) != 0;
            IReadOnlyList<FileSystemEntry> children = [];
            IReadOnlyList<byte>? content = null;
            var metadataValid = resolve(indirectAddress, 0, out _);
            if (isDirectory && metadataValid)
            {
                try { children = ReadDirectory(image, indirectAddress, resolve, visited, warnings, depth + 1).Children; }
                catch (InvalidDataException exception) { warnings.Add($"{name}: {exception.Message}"); metadataValid = false; }
            }
            else if (!isDirectory)
            {
                content = ReadFile(image, indirectAddress, length, resolve, name, warnings, ref metadataValid);
            }
            var type = HasRiscOsTimestamp(load) ? (load >> 8) & 0xFFF : 0u;
            var comment = HasRiscOsTimestamp(load)
                ? $"RISC OS file type &{type:X3}, load &{load:X8}, execute &{execute:X8}"
                : $"ADFS load &{load:X8}, execute &{execute:X8}";
            entries.Add(new(name, isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File,
                isDirectory ? 0 : length, ReadTimestamp(load, execute), comment, attributes, indirectAddress,
                metadataValid, children, content));
        }

        var tail = 5 + EntryCount * EntrySize;
        var title = DecodeName(directory.AsSpan(tail + 6, 19));
        var directoryName = DecodeName(directory.AsSpan(tail + 25, 10));
        return new(directoryName, title, entries
            .OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static IReadOnlyList<byte>? ReadFile(SectorImage image, int startBlock, uint length, AddressResolver resolve,
        string name,
        List<string> warnings, ref bool metadataValid)
    {
        if (length == 0) return [];
        if (length > int.MaxValue || startBlock <= 0 || !resolve(startBlock, 0, out _))
        {
            warnings.Add($"{name}: the ADFS data address or length is invalid.");
            metadataValid = false;
            return null;
        }
        var output = new byte[(int)length];
        var copied = 0;
        while (copied < output.Length)
        {
            if (!resolve(startBlock, copied, out var byteOffset))
            {
                warnings.Add($"{name}: the ADFS data address is invalid.");
                metadataValid = false;
                break;
            }
            var blockNumber = checked((int)(byteOffset / BlockSize));
            var offsetInBlock = checked((int)(byteOffset % BlockSize));
            if (!image.TryGetBlock(blockNumber, out var block))
            {
                warnings.Add($"{name}: data sector {blockNumber} is missing.");
                metadataValid = false;
                break;
            }
            var count = Math.Min(block.Data.Count - offsetInBlock, output.Length - copied);
            block.Data.Skip(offsetInBlock).Take(count).ToArray().CopyTo(output, copied);
            copied += count;
        }
        return output;
    }

    private static bool TryReadDirectory(SectorImage image, int startBlock, AddressResolver resolve,
        out byte[] directory)
    {
        if (!TryReadBytes(image, startBlock, resolve, DirectorySize, out directory)) return false;
        var header = System.Text.Encoding.ASCII.GetString(directory, 1, 4);
        var footer = System.Text.Encoding.ASCII.GetString(directory, DirectorySize - 5, 4);
        return (header is "Hugo" or "Nick") && footer == header && directory[0] == directory[DirectorySize - 6];
    }

    private static bool TryReadBytes(SectorImage image, int indirectAddress, AddressResolver resolve, int length,
        out byte[] output)
    {
        output = new byte[length];
        var copied = 0;
        while (copied < length)
        {
            if (!resolve(indirectAddress, copied, out var byteOffset)) return false;
            var blockNumber = checked((int)(byteOffset / BlockSize));
            var offsetInBlock = checked((int)(byteOffset % BlockSize));
            if (!image.TryGetBlock(blockNumber, out var block) || block.Data.Count != BlockSize) return false;
            var count = Math.Min(BlockSize - offsetInBlock, length - copied);
            block.Data.Skip(offsetInBlock).Take(count).ToArray().CopyTo(output, copied);
            copied += count;
        }
        return true;
    }

    private static Layout CreateLayout(SectorImage image)
    {
        if (AcornFileCoreNewMap.TryCreate(image, out var map) && map is not null)
            return new(map.Record.RootAddress, map.Record.DiscName, map.ReadFreeBytes(), map.TryResolveByteOffset);
        var oldMap = image.GetBlock(0).Span;
        return new(4, ReadOldMapName(oldMap), ReadOldMapFreeBytes(oldMap, image.Capacity),
            (int address, long offset, out long physicalOffset) =>
            {
                physicalOffset = (long)address * FileCoreUnitSize + offset;
                return address > 0 && offset >= 0 && physicalOffset >= 0 && physicalOffset < image.Capacity;
            });
    }

    private delegate bool AddressResolver(int indirectAddress, long objectByteOffset, out long physicalByteOffset);
    private sealed record Layout(int RootAddress, string VolumeName, long FreeBytes, AddressResolver Resolve);

    private static string ReadOldMapName(ReadOnlySpan<byte> map)
    {
        if (map.Length < 507) return "";
        Span<byte> name = stackalloc byte[10];
        for (var index = 0; index < 5; index++)
        {
            name[index * 2] = map[247 + index];
            name[index * 2 + 1] = map[502 + index];
        }
        return DecodeName(name);
    }

    private static long ReadOldMapFreeBytes(ReadOnlySpan<byte> map, long capacity)
    {
        if (map.Length < 502) return 0;
        long sectors = 0;
        for (var index = 0; index < 82; index++) sectors += ReadUInt24(map, 256 + index * 3);
        return Math.Min(capacity, sectors * 256L);
    }

    private static int ReadUInt24(ReadOnlySpan<byte> data, int offset) =>
        data[offset] | data[offset + 1] << 8 | data[offset + 2] << 16;

    private static bool HasRiscOsTimestamp(uint load) => (load & 0xFFF00000) == 0xFFF00000;

    private static DateTimeOffset? ReadTimestamp(uint load, uint execute)
    {
        if (!HasRiscOsTimestamp(load)) return null;
        var centiseconds = ((ulong)(load & 0xFF) << 32) | execute;
        try { return RiscOsEpoch.AddMilliseconds(centiseconds * 10d); }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    private static string DecodeName(ReadOnlySpan<byte> bytes)
    {
        Span<byte> clean = stackalloc byte[bytes.Length];
        for (var index = 0; index < bytes.Length; index++) clean[index] = (byte)(bytes[index] & 0x7F);
        return System.Text.Encoding.ASCII.GetString(clean).TrimEnd('\0', '\r', ' ');
    }
}
