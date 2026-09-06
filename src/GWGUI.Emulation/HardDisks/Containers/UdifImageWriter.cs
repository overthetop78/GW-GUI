using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Autonomous UDIF v4: XML block map, RAW or zlib runs and explicit zero runs.</summary>
public static class UdifImageWriter
{
    private const int ChunkBytes = 8 << 20;
    private sealed record Run(uint Type, long Sector, long Count, long Offset, long Length);

    public static void Validate(long capacity)
    {
        if (capacity < 512 || capacity % 512 != 0 || capacity > 1L << 40)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null, bool compressed = false)
    {
        Validate(capacity); ContainerValidation.Validate(destination, capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        var units = SparseImageContent.AllocatedUnits(content, ChunkBytes);
        var runs = new List<Run>();
        var buffer = new byte[ChunkBytes];
        long cursor = 0;
        foreach (var unit in units)
        {
            var start = unit * ChunkBytes;
            if (start > cursor) runs.Add(new(0, cursor / 512, (start - cursor) / 512, 0, 0));
            var length = (int)Math.Min(ChunkBytes, capacity - start);
            content.Position = start; content.ReadExactly(buffer.AsSpan(0, length));
            var physical = destination.Position;
            uint type = 1;
            if (compressed)
            {
                using var encoded = new MemoryStream();
                using (var zlib = new ZLibStream(encoded, CompressionLevel.Optimal, leaveOpen: true)) zlib.Write(buffer, 0, length);
                if (encoded.Length < length)
                { type = 0x80000005; encoded.Position = 0; encoded.CopyTo(destination); }
                else destination.Write(buffer, 0, length);
            }
            else destination.Write(buffer, 0, length);
            runs.Add(new(type, start / 512, length / 512, physical, destination.Position - physical));
            cursor = start + length;
        }
        if (cursor < capacity) runs.Add(new(0, cursor / 512, (capacity - cursor) / 512, 0, 0));
        runs.Add(new(uint.MaxValue, capacity / 512, 0, 0, 0));
        var map = new byte[checked(204 + runs.Count * 40)];
        U32(map, 0, 0x6d697368); U32(map, 4, 1); U64(map, 16, (ulong)(capacity / 512));
        U32(map, 32, ChunkBytes / 512); U32(map, 36, 0xfffffffe); U32(map, 200, (uint)runs.Count);
        // Checksum type zero explicitly means absent, not a fabricated CRC.
        for (var i = 0; i < runs.Count; i++)
        {
            var run = runs[i]; var at = 204 + i * 40;
            U32(map, at, run.Type); U64(map, at + 8, (ulong)run.Sector); U64(map, at + 16, (ulong)run.Count);
            U64(map, at + 24, (ulong)run.Offset); U64(map, at + 32, (ulong)run.Length);
        }
        var xml = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n" +
            "<plist version=\"1.0\"><dict><key>resource-fork</key><dict><key>blkx</key><array><dict>" +
            "<key>Attributes</key><string>0x0050</string><key>ID</key><string>0</string>" +
            "<key>Name</key><string>whole disk</string><key>Data</key><data>" + Convert.ToBase64String(map) +
            "</data></dict></array></dict></dict></plist>");
        var dataLength = destination.Position; destination.Write(xml);
        var footer = new byte[512]; U32(footer, 0, 0x6b6f6c79); U32(footer, 4, 4); U32(footer, 8, 512); U32(footer, 12, 1);
        U64(footer, 32, (ulong)dataLength); U32(footer, 56, 1); U32(footer, 60, 1);
        Guid.NewGuid().TryWriteBytes(footer.AsSpan(64, 16));
        U64(footer, 216, (ulong)dataLength); U64(footer, 224, (ulong)xml.Length);
        U32(footer, 488, 1); U64(footer, 492, (ulong)(capacity / 512));
        destination.Write(footer); destination.Flush();
    }

    private static void U32(byte[] bytes, int at, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(at), value);
    private static void U64(byte[] bytes, int at, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(at), value);
}
