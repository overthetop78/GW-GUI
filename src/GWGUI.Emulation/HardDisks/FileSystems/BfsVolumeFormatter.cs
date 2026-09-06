using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Little-endian BFS boot filesystem. This is distinct from the BeFS filesystem.</summary>
public static class BfsVolumeFormatter
{
    public const long MaximumCapacity = 1L << 32;

    public static void Validate(long capacity, string label, int inodes = 256, string fileSystemName = "")
    {
        if (inodes is < 8 or > 512 || inodes % 8 != 0)
            throw new ArgumentOutOfRangeException(nameof(inodes));
        if (capacity < (33L + inodes / 8) * 512 || capacity > MaximumCapacity || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        ValidateName(label);
        ValidateName(fileSystemName);
    }

    public static void Format(Stream volume, string label, int inodes = 256, string fileSystemName = "")
    {
        if (!volume.CanSeek || !volume.CanWrite) throw new ArgumentException("A writable seekable volume is required.");
        Validate(volume.Length, label, inodes, fileSystemName);
        var firstDataBlock = 1 + inodes / 8;
        var directoryBlocks = (inodes * 16 + 511) / 512;
        // Clear the inode table and the entire reserved root extent, including unused directory entries.
        var metadata = new byte[(firstDataBlock + directoryBlocks) * 512];
        var super = metadata.AsSpan(0, 512);
        Put32(super, 0, 0x1badface);
        Put32(super, 4, (uint)(firstDataBlock * 512));
        Put32(super, 8, (uint)(volume.Length - 1));
        for (var offset = 12; offset <= 24; offset += 4) Put32(super, offset, uint.MaxValue);
        Encoding.ASCII.GetBytes(fileSystemName, super.Slice(28, 6));
        Encoding.ASCII.GetBytes(label, super.Slice(34, 6));

        var root = metadata.AsSpan(512, 64);
        BinaryPrimitives.WriteUInt16LittleEndian(root, 2);
        Put32(root, 4, (uint)firstDataBlock);
        Put32(root, 8, (uint)(firstDataBlock + directoryBlocks - 1));
        Put32(root, 12, (uint)(firstDataBlock * 512 + 31));
        Put32(root, 16, 2);
        Put32(root, 20, 0x41ed);
        Put32(root, 32, 2);
        var directory = metadata.AsSpan(firstDataBlock * 512, directoryBlocks * 512);
        BinaryPrimitives.WriteUInt16LittleEndian(directory, 2);
        directory[2] = (byte)'.';
        BinaryPrimitives.WriteUInt16LittleEndian(directory[16..], 2);
        directory[18] = directory[19] = (byte)'.';
        volume.Position = 0;
        volume.Write(metadata);
        volume.Flush();
    }

    private static void ValidateName(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length > 6 || value.Any(c => c < 32 || c > 126))
            throw new ArgumentException("BFS labels contain at most six printable ASCII bytes.");
    }

    private static void Put32(Span<byte> bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[offset..], value);
}
