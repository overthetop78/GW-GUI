using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Partitioning;

public static class AhdiPartitionWriter
{
    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes)
    {
        var logical = volumes.Where(v => v.AhdiLogical).OrderBy(v => v.OffsetBytes).ToArray();
        var primary = volumes.Where(v => !v.AhdiLogical).ToArray();
        if (capacity < 512 || capacity % 512 != 0 || capacity / 512 > uint.MaxValue ||
            primary.Length + (logical.Length > 0 ? 1 : 0) > 4 || volumes.Any(v => v.MbrLogical))
            throw new ArgumentException("AHDI supports four primary entries, including the XGM container when present.");
        long end = 512;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            var type = Type(volume);
            if (volume.OffsetBytes < end || volume.LengthBytes <= 0 || volume.OffsetBytes % 512 != 0 || volume.LengthBytes % 512 != 0 ||
                volume.OffsetBytes > capacity || volume.LengthBytes > capacity - volume.OffsetBytes ||
                (volume.PartitionType is null && !new[] { "none", "fat16-adapted" }.Contains(volume.FileSystemId, StringComparer.OrdinalIgnoreCase)) ||
                type.Length != 3 || type.Any(c => c < 32 || c > 126) || type == "XGM")
                throw new ArgumentException("Invalid AHDI primary partition.");
            end = volume.OffsetBytes + volume.LengthBytes;
        }
        if (logical.Length == 0) return;
        var first = logical[0].OffsetBytes - 512;
        var last = logical[^1].OffsetBytes + logical[^1].LengthBytes;
        if (first < 512 || primary.Any(v => v.OffsetBytes < last && v.OffsetBytes + v.LengthBytes > first))
            throw new ArgumentException("An XGM container cannot overlap primary partitions.");
        for (var i = 1; i < logical.Length; i++)
            if (logical[i].OffsetBytes - 512 < logical[i - 1].OffsetBytes + logical[i - 1].LengthBytes)
                throw new ArgumentException("Each logical partition needs a reserved auxiliary root sector.");
    }
    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes)
    {
        Validate(disk.Length, volumes);
        var root = new byte[512]; U32(root, 0x1c2, (uint)(disk.Length / 512));
        var primary = volumes.Where(v => !v.AhdiLogical).ToArray();
        var logical = volumes.Where(v => v.AhdiLogical).OrderBy(v => v.OffsetBytes).ToArray();
        for (var i = 0; i < primary.Length; i++)
            Entry(root, i, Type(primary[i]), primary[i].OffsetBytes / 512, primary[i].LengthBytes / 512);
        if (logical.Length > 0)
        {
            var first = logical[0].OffsetBytes / 512 - 1;
            var end = (logical[^1].OffsetBytes + logical[^1].LengthBytes) / 512;
            Entry(root, primary.Length, "XGM", first, end - first);
            for (var i = 0; i < logical.Length; i++)
            {
                var auxiliary = new byte[512];
                Entry(auxiliary, 0, Type(logical[i]), 1, logical[i].LengthBytes / 512);
                if (i + 1 < logical.Length)
                {
                    var next = logical[i + 1];
                    Entry(auxiliary, 1, "XGM", next.OffsetBytes / 512 - 1 - first, next.LengthBytes / 512 + 1);
                }
                disk.Position = logical[i].OffsetBytes - 512; disk.Write(auxiliary);
            }
        }
        disk.Position = 0; disk.Write(root); disk.Flush();
    }
    private static string Type(DiskVolumePlan volume) => volume.PartitionType ?? (volume.LengthBytes < 32L << 20 ? "GEM" : "BGM");
    private static void Entry(byte[] bytes, int index, string type, long start, long length)
    {
        var offset = 0x1c6 + index * 12; bytes[offset] = 1;
        Encoding.ASCII.GetBytes(type).CopyTo(bytes, offset + 1);
        U32(bytes, offset + 4, checked((uint)start)); U32(bytes, offset + 8, checked((uint)length));
    }
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
}
