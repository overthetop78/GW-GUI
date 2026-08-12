using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.FileSystems.Ucsd;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les volumes UCSD p-System.</summary>
public sealed class UcsdFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.Ucsd;
    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DiskImageFormatIds.UcsdIbmMfm };

    /// <inheritdoc />
    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != UcsdFileSystemLayout.BlockSize || !image.TryGetBlock(UcsdFileSystemLayout.DirectoryBlock, out var block)) return false;
        var data = block.Data is byte[] bytes ? bytes.AsSpan() : block.Data.ToArray().AsSpan();
        if (data.Length < UcsdFileSystemLayout.EntrySize) return false;
        var detection = DetectByteOrder(data);
        if (!detection.Success) return false;
        var endDirectory = ReadUInt16(data, UcsdFileSystemLayout.DirectoryEndOffset, detection.ByteOrder);
        var nameLength = data[UcsdFileSystemLayout.VolumeNameOffset];
        var totalBlocks = ReadUInt16(data, UcsdFileSystemLayout.TotalBlocksOffset, detection.ByteOrder);
        var fileCount = ReadUInt16(data, UcsdFileSystemLayout.FileCountOffset, detection.ByteOrder);
        return endDirectory is UcsdFileSystemLayout.ShortDirectoryEnd or UcsdFileSystemLayout.LongDirectoryEnd && nameLength is > 0 and <= UcsdFileSystemLayout.MaximumVolumeNameLength && totalBlocks <= image.BlockCount && fileCount <= UcsdFileSystemLayout.MaximumFileCount && IsName(data.Slice(UcsdFileSystemLayout.VolumeNameOffset + 1, nameLength));
    }

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!image.TryGetBlock(UcsdFileSystemLayout.DirectoryBlock, out var directoryBlock)) throw UcsdFileSystemExceptions.MissingDirectory(UcsdFileSystemLayout.DirectoryBlock, 0);
        if (!CanRead(image)) throw UcsdFileSystemExceptions.UnknownByteOrder(directoryBlock.Data.ToArray());
        var directory = ReadBlocks(image, UcsdFileSystemLayout.DirectoryBlock, UcsdFileSystemLayout.ShortDirectoryBlockCount, out var directoryComplete);
        var byteOrder = DetectByteOrder(directory).ByteOrder;
        var endDirectory = ReadUInt16(directory, UcsdFileSystemLayout.DirectoryEndOffset, byteOrder);
        if (endDirectory == UcsdFileSystemLayout.LongDirectoryEnd) directory = ReadBlocks(image, UcsdFileSystemLayout.DirectoryBlock, UcsdFileSystemLayout.LongDirectoryBlockCount, out directoryComplete);
        var volumeName = DecodeName(directory.AsSpan(UcsdFileSystemLayout.VolumeNameOffset, UcsdFileSystemLayout.VolumeNameFieldLength), UcsdFileSystemLayout.MaximumVolumeNameLength);
        var totalBlocks = ReadUInt16(directory, UcsdFileSystemLayout.TotalBlocksOffset, byteOrder);
        var declaredFiles = ReadUInt16(directory, UcsdFileSystemLayout.FileCountOffset, byteOrder);
        var volumeDate = DecodeDate(ReadUInt16(directory, UcsdFileSystemLayout.VolumeDateOffset, byteOrder));
        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        if (!directoryComplete) warnings.Add(UcsdFileSystemExceptions.IncompleteRange(UcsdFileSystemLayout.DirectoryBlock, endDirectory - UcsdFileSystemLayout.DirectoryBlock, directory.Length));

        var usedBlocks = (int)endDirectory;
        var maxEntries = Math.Min(declaredFiles, (ushort)Math.Min(UcsdFileSystemLayout.MaximumFileCount, (directory.Length - UcsdFileSystemLayout.EntrySize) / UcsdFileSystemLayout.EntrySize));
        for (var index = 0; index < maxEntries; index++)
        {
            var offset = (index + 1) * UcsdFileSystemLayout.EntrySize;
            var entry = directory.AsSpan(offset, UcsdFileSystemLayout.EntrySize);
            var firstBlock = ReadUInt16(entry, UcsdFileSystemLayout.EntryFirstBlockOffset, byteOrder);
            var lastBlock = ReadUInt16(entry, UcsdFileSystemLayout.EntryLastBlockOffset, byteOrder);
            if (firstBlock == 0 && lastBlock == 0) continue;
            var name = DecodeName(entry.Slice(UcsdFileSystemLayout.EntryNameOffset, UcsdFileSystemLayout.EntryNameFieldLength), UcsdFileSystemLayout.MaximumFileNameLength);
            if (name.Length == 0)
            {
                warnings.Add(UcsdFileSystemExceptions.InvalidEntry(index + 1, name, firstBlock, lastBlock));
                continue;
            }
            if (lastBlock < firstBlock || lastBlock > totalBlocks)
            {
                warnings.Add(UcsdFileSystemExceptions.InvalidEntry(index + 1, name, firstBlock, lastBlock));
                continue;
            }

            var blocks = lastBlock - firstBlock;
            var lastBytes = ReadUInt16(entry, UcsdFileSystemLayout.EntryLastBlockBytesOffset, byteOrder);
            var size = blocks == 0 ? 0 : checked((long)(blocks - 1) * UcsdFileSystemLayout.BlockSize + Math.Min(lastBytes == 0 ? UcsdFileSystemLayout.BlockSize : lastBytes, UcsdFileSystemLayout.BlockSize));
            var content = ReadFile(image, firstBlock, blocks, size, out var complete);
            if (!complete) warnings.Add(Definitions.FileSystemWarningMessages.MissingDataBlocks(name));
            var kind = (UcsdFileKind)(ReadUInt16(entry, UcsdFileSystemLayout.EntryKindOffset, byteOrder) & UcsdFileSystemLayout.FileKindMask);
            entries.Add(new(name, FileSystemEntryKind.File, size, DecodeDate(ReadUInt16(entry, UcsdFileSystemLayout.EntryDateOffset, byteOrder)), UcsdFileKindNames.Get(kind), 0, firstBlock, complete, [], content));
            usedBlocks += blocks;
        }

        if (declaredFiles != entries.Count) warnings.Add($"The UCSD directory declares {declaredFiles} files but {entries.Count} valid entries were found.");
        var freeBlocks = Math.Max(0, totalBlocks - usedBlocks);
        return new FileSystemVolume(volumeName, Definitions.FileSystemIds.Ucsd, (long)totalBlocks * UcsdFileSystemLayout.BlockSize, (long)freeBlocks * UcsdFileSystemLayout.BlockSize,
            null, volumeDate, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    /// <summary>Détecte explicitement l'ordre des octets du répertoire.</summary>
    internal static UcsdByteOrderDetection DetectByteOrder(ReadOnlySpan<byte> directory)
    {
        if (directory.Length < 4) return default;
        if (directory[2] is UcsdFileSystemLayout.ShortDirectoryEnd or UcsdFileSystemLayout.LongDirectoryEnd && directory[3] == 0) return new(true, UcsdByteOrder.LittleEndian);
        if (directory[3] is UcsdFileSystemLayout.ShortDirectoryEnd or UcsdFileSystemLayout.LongDirectoryEnd && directory[2] == 0) return new(true, UcsdByteOrder.BigEndian);
        return default;
    }

    private static byte[] ReadBlocks(SectorImage image, int first, int count, out bool complete)
    {
        var result = new byte[count * UcsdFileSystemLayout.BlockSize];
        complete = true;
        for (var index = 0; index < count; index++)
        {
            if (!image.TryGetBlock(first + index, out var block) || block.Data.Count < UcsdFileSystemLayout.BlockSize) { complete = false; continue; }
            block.Data.Take(UcsdFileSystemLayout.BlockSize).ToArray().CopyTo(result, index * UcsdFileSystemLayout.BlockSize);
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
    /// <summary>Lit un entier selon l'ordre d'octets détecté.</summary>
    internal static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset, UcsdByteOrder byteOrder) => byteOrder == UcsdByteOrder.LittleEndian
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
}
