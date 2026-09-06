using System.Buffers.Binary;
using System.IO.Compression;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>cloop v2 with zlib blocks and a big-endian 64-bit offset table.</summary>
public static class CompressedLoopImageWriter
{
    public static void Validate(long capacity, int blockBytes = 65536)
    {
        if (blockBytes < 512 || blockBytes > 262144 || (blockBytes & (blockBytes - 1)) != 0 ||
            capacity < blockBytes || capacity % blockBytes != 0 || capacity / blockBytes > 0x400000)
            throw new ArgumentException("cloop requires complete power-of-two blocks and at most 4,194,304 blocks.");
    }

    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null, int blockBytes = 65536)
    {
        Validate(capacity, blockBytes); ContainerValidation.Validate(destination, capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        var allocated = SparseImageContent.AllocatedUnits(content, blockBytes);
        var blocks = checked((int)(capacity / blockBytes));
        var header = new byte[136];
        // The historical header is a format marker. The writer never executes it.
        "#!/bin/sh\n#V2.0 Format\nmodprobe cloop file=$0 && mount -r -t iso9660 /dev/cloop $1\n"u8.CopyTo(header);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(128), (uint)blockBytes);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(132), (uint)blocks);
        var offsets = new byte[checked((blocks + 1) * 8)];
        var buffer = new byte[blockBytes]; var zero = Compress(buffer);
        destination.Position = header.Length + offsets.Length;
        for (var block = 0; block < blocks; block++)
        {
            BinaryPrimitives.WriteUInt64BigEndian(offsets.AsSpan(block * 8), (ulong)destination.Position);
            byte[] encoded;
            if (allocated.Contains(block))
            {
                content.Position = block * (long)blockBytes; content.ReadExactly(buffer); encoded = Compress(buffer);
            }
            else encoded = zero;
            destination.Write(encoded);
        }
        BinaryPrimitives.WriteUInt64BigEndian(offsets.AsSpan(blocks * 8), (ulong)destination.Position);
        destination.Position = 0; destination.Write(header); destination.Write(offsets); destination.Flush();
    }

    private static byte[] Compress(byte[] bytes)
    {
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true)) zlib.Write(bytes);
        return output.ToArray();
    }
}
