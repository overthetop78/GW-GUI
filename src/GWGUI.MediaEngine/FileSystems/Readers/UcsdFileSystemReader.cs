using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class UcsdFileSystemReader : IFileSystemReader
{
    private const int BlockSize = 512;
    private const int DirectoryBlock = 2;
    private const int EntrySize = 26;

    public string Id => Definitions.FileSystemIds.Ucsd;
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.UcsdIbmMfm };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != BlockSize || !image.TryGetBlock(DirectoryBlock, out var block)) return false;
        var data = block.Data is byte[] bytes ? bytes.AsSpan() : block.Data.ToArray().AsSpan();
        if (data.Length < EntrySize) return false;
        var littleEndian = DetectByteOrder(data);
        if (littleEndian is null) return false;
        var endDirectory = ReadUInt16(data, 2, littleEndian.Value);
        var nameLength = data[6];
        var totalBlocks = ReadUInt16(data, 14, littleEndian.Value);
        var fileCount = ReadUInt16(data, 16, littleEndian.Value);
        return endDirectory is 6 or 10 && nameLength is > 0 and <= 7 && totalBlocks <= image.BlockCount && fileCount <= 77 && IsName(data.Slice(7, nameLength));
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a UCSD p-System file system.");
        var directory = ReadBlocks(image, DirectoryBlock, 4, out var directoryComplete);
        var littleEndian = DetectByteOrder(directory)!.Value;
        var endDirectory = ReadUInt16(directory, 2, littleEndian);
        if (endDirectory == 10) directory = ReadBlocks(image, DirectoryBlock, 8, out directoryComplete);
        var volumeName = DecodeName(directory.AsSpan(6, 8), 7);
        var totalBlocks = ReadUInt16(directory, 14, littleEndian);
        var declaredFiles = ReadUInt16(directory, 16, littleEndian);
        var volumeDate = DecodeDate(ReadUInt16(directory, 20, littleEndian));
        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        if (!directoryComplete) warnings.Add("One or more UCSD directory blocks are missing.");

        var usedBlocks = (int)endDirectory;
        var maxEntries = Math.Min(declaredFiles, (ushort)Math.Min(77, (directory.Length - EntrySize) / EntrySize));
        for (var index = 0; index < maxEntries; index++)
        {
            var offset = (index + 1) * EntrySize;
            var entry = directory.AsSpan(offset, EntrySize);
            var firstBlock = ReadUInt16(entry, 0, littleEndian);
            var lastBlock = ReadUInt16(entry, 2, littleEndian);
            if (firstBlock == 0 && lastBlock == 0) continue;
            var name = DecodeName(entry.Slice(6, 16), 15);
            if (name.Length == 0)
            {
                warnings.Add($"UCSD directory entry {index + 1} has an invalid or empty name.");
                continue;
            }
            if (lastBlock < firstBlock || lastBlock > totalBlocks)
            {
                warnings.Add($"{name}: invalid block range {firstBlock}..{lastBlock}.");
                continue;
            }

            var blocks = lastBlock - firstBlock;
            var lastBytes = ReadUInt16(entry, 22, littleEndian);
            var size = blocks == 0 ? 0 : checked((long)(blocks - 1) * BlockSize + Math.Min(lastBytes == 0 ? BlockSize : lastBytes, BlockSize));
            var content = ReadFile(image, firstBlock, blocks, size, out var complete);
            if (!complete) warnings.Add(Definitions.FileSystemWarningMessages.MissingDataBlocks(name));
            var kind = ReadUInt16(entry, 4, littleEndian) & 0x0f;
            entries.Add(new(name, FileSystemEntryKind.File, size, DecodeDate(ReadUInt16(entry, 24, littleEndian)),
                FileKindName(kind), 0, firstBlock, complete, [], content));
            usedBlocks += blocks;
        }

        if (declaredFiles != entries.Count) warnings.Add($"The UCSD directory declares {declaredFiles} files but {entries.Count} valid entries were found.");
        var freeBlocks = Math.Max(0, totalBlocks - usedBlocks);
        return new FileSystemVolume(volumeName, Definitions.FileSystemDisplayNames.Ucsd, (long)totalBlocks * BlockSize, (long)freeBlocks * BlockSize,
            null, volumeDate, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static bool? DetectByteOrder(ReadOnlySpan<byte> directory)
    {
        if (directory.Length < 4) return null;
        if (directory[2] is 6 or 10 && directory[3] == 0) return true;
        if (directory[3] is 6 or 10 && directory[2] == 0) return false;
        return null;
    }

    private static byte[] ReadBlocks(SectorImage image, int first, int count, out bool complete)
    {
        var result = new byte[count * BlockSize];
        complete = true;
        for (var index = 0; index < count; index++)
        {
            if (!image.TryGetBlock(first + index, out var block) || block.Data.Count < BlockSize) { complete = false; continue; }
            block.Data.Take(BlockSize).ToArray().CopyTo(result, index * BlockSize);
        }
        return result;
    }

    private static byte[] ReadFile(SectorImage image, int first, int count, long size, out bool complete)
    {
        var data = ReadBlocks(image, first, count, out complete);
        return data.AsSpan(0, checked((int)Math.Min(size, data.Length))).ToArray();
    }

    private static string DecodeName(ReadOnlySpan<byte> field, int maximum)
    {
        var length = field[0];
        if (length == 0 || length > maximum || length >= field.Length || !IsName(field.Slice(1, length))) return string.Empty;
        return System.Text.Encoding.ASCII.GetString(field.Slice(1, length));
    }

    private static bool IsName(ReadOnlySpan<byte> name)
    {
        if (name.Length == 0) return false;
        foreach (var value in name) if (value is < 0x20 or >= 0x7f) return false;
        return true;
    }
    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset, bool littleEndian) => littleEndian
        ? (ushort)(data[offset] | data[offset + 1] << BitPrimitives.BitsPerByte)
        : (ushort)(data[offset] << BitPrimitives.BitsPerByte | data[offset + 1]);

    private static DateTimeOffset? DecodeDate(ushort value)
    {
        if (value == 0) return null;
        var day = value & 0x1f;
        var month = value >> 5 & 0x0f;
        var shortYear = value >> 9 & 0x7f;
        var year = shortYear >= 70 ? 1900 + shortYear : 2000 + shortYear;
        try { return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero); }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    private static string FileKindName(int kind) => kind switch
    {
        1 => "UCSD external disk file",
        2 => "UCSD code file",
        3 => "UCSD text file",
        4 => "UCSD info file",
        5 => "UCSD data file",
        6 => "UCSD graphics file",
        7 => "UCSD photo file",
        8 => "UCSD secure directory",
        _ => "UCSD untyped file"
    };
}
