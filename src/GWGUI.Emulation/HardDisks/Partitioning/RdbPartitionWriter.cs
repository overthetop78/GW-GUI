using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Partitioning;

public static class RdbPartitionWriter
{
    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes)
        => Validate(capacity, volumes, new RdbGeometry());

    public static DiskFormatRegistry.PartitionTable Describe(string id, RdbGeometry geometry)
        => new(id, (capacity, volumes) => Validate(capacity, volumes, geometry),
            (disk, volumes) => Write(disk, volumes, geometry));

    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes, RdbGeometry geometry)
    {
        var CylinderBytes = geometry.CylinderBytes;
        var reserved = ReservedCylinders(volumes.Count, CylinderBytes);
        if (capacity < CylinderBytes * (reserved + 1) || capacity % CylinderBytes != 0 ||
            capacity / CylinderBytes > uint.MaxValue || capacity / 512 > uint.MaxValue || volumes.Any(v=>v.MbrLogical || v.AhdiLogical))
            throw new ArgumentException("Invalid RDB geometry or partition count.");
        long end = CylinderBytes * reserved;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            if (volume.OffsetBytes < end || volume.OffsetBytes % CylinderBytes != 0 || volume.LengthBytes <= 0 ||
                volume.LengthBytes % CylinderBytes != 0 || volume.OffsetBytes > capacity || volume.LengthBytes > capacity - volume.OffsetBytes ||
                volume.EffectivePartitionName.Length is < 1 or > 30 || volume.EffectivePartitionName.Any(c => c < 32 || c > 255) ||
                (volume.PartitionType is null && DosType(volume.FileSystemId) is null) ||
                (volume.PartitionType is { } type && (type.Length != 4 || type.Any(c => c > 255))))
                throw new ArgumentException("Invalid RDB partition.");
            end = volume.OffsetBytes + volume.LengthBytes;
        }
    }
    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes)
        => Write(disk, volumes, new RdbGeometry());

    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes, RdbGeometry geometry)
    {
        Validate(disk.Length, volumes, geometry);
        var CylinderBytes = geometry.CylinderBytes;
        var reserved = ReservedCylinders(volumes.Count, CylinderBytes);
        var cylinders = (uint)(disk.Length / CylinderBytes);
        var rdb = new byte[512]; "RDSK"u8.CopyTo(rdb);
        Set(rdb, 4, 64); Set(rdb, 12, 7); Set(rdb, 16, 512); Set(rdb, 20, 7);
        Set(rdb, 24, uint.MaxValue); Set(rdb, 28, volumes.Count == 0 ? uint.MaxValue : 1);
        Set(rdb, 32, uint.MaxValue); Set(rdb, 36, uint.MaxValue);
        Set(rdb, 64, cylinders); Set(rdb, 68, (uint)geometry.SectorsPerTrack); Set(rdb, 72, (uint)geometry.Heads);
        Set(rdb, 132, checked((uint)(reserved * CylinderBytes / 512 - 1))); Set(rdb, 136, (uint)reserved);
        Set(rdb, 140, cylinders - 1); Set(rdb, 144, (uint)(CylinderBytes / 512)); Set(rdb, 152, (uint)volumes.Count);
        "GWGUI   "u8.CopyTo(rdb.AsSpan(160)); "Virtual HDD     "u8.CopyTo(rdb.AsSpan(168));
        Checksum(rdb); disk.Position = 0;
        // Only actual metadata blocks need initialization; padding stays unallocated in sparse streams.
        disk.Write(rdb);
        for (var i = 0; i < volumes.Count; i++)
        {
            var volume = volumes[i]; var part = new byte[512]; "PART"u8.CopyTo(part);
            Set(part, 4, 64); Set(part, 12, 7); Set(part, 16, i + 1 < volumes.Count ? (uint)i + 2 : uint.MaxValue);
            Set(part, 20, volume.Active ? 1u : 0); part[36] = (byte)volume.EffectivePartitionName.Length;
            Encoding.Latin1.GetBytes(volume.EffectivePartitionName).CopyTo(part, 37);
            Set(part, 128, 16); Set(part, 132, 128); Set(part, 140, (uint)geometry.Heads); Set(part, 144, 1);
            Set(part, 148, (uint)geometry.SectorsPerTrack); Set(part, 152, 2);
            Set(part, 164, (uint)(volume.OffsetBytes / CylinderBytes)); Set(part, 168, (uint)((volume.OffsetBytes + volume.LengthBytes) / CylinderBytes - 1));
            Set(part, 172, 30); Set(part, 180, 0x1fe00); Set(part, 184, 0x7ffffffe);
            if (volume.PartitionType is { } type) Encoding.Latin1.GetBytes(type).CopyTo(part, 192);
            else Set(part, 192, DosType(volume.FileSystemId)!.Value);
            Checksum(part); disk.Position = (i + 1) * 512; disk.Write(part);
        }
        disk.Flush();
    }
    private static void Set(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    private static long ReservedCylinders(int partitions, long cylinderBytes)
        => Math.Max(1, ((partitions + 1L) * 512 + cylinderBytes - 1) / cylinderBytes);
    private static uint? DosType(string id)=>id.ToLowerInvariant() switch
    {
        "ofs"=>0x444f5300u, "none" or "ffs"=>0x444f5301u,
        "ofs-intl"=>0x444f5302u,"ffs-intl"=>0x444f5303u,
        "ofs-dircache"=>0x444f5304u,"ffs-dircache"=>0x444f5305u,
        "ofs-longnames"=>0x444f5306u,"ffs-longnames"=>0x444f5307u,
        "pfs3"=>0x50465303u,
        _=>null
    };
    private static void Checksum(byte[] bytes)
    {
        uint sum = 0; for (var i = 0; i < 256; i += 4) sum = unchecked(sum + BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(i)));
        Set(bytes, 8, unchecked(0u - sum));
    }
}
