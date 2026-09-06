using System.Buffers.Binary;
using System.Globalization;

namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>SGI volume header; slots 8 and 10 reserve the header and entire disk.</summary>
public static class SgiDisklabelWriter
{
    private const long HeaderBytes = 4096 * 512L;
    public static DiskFormatRegistry.PartitionTable Describe(string id, LegacyDiskGeometry geometry)
        => new(id, (size, volumes) => Validate(size, volumes, geometry), (disk, volumes) => Write(disk, volumes, geometry));

    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes) => Validate(capacity, volumes, new());
    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes, LegacyDiskGeometry geometry)
    {
        _ = geometry.Validate(capacity);
        if (capacity < HeaderBytes || volumes.Count > 14) throw new ArgumentException("Invalid SGI disk size or partition count.");
        long end = HeaderBytes;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            if (volume.OffsetBytes < end || volume.OffsetBytes % 512 != 0 || volume.LengthBytes <= 0 ||
                volume.LengthBytes % 512 != 0 || volume.OffsetBytes > capacity || volume.LengthBytes > capacity - volume.OffsetBytes ||
                volume.MbrLogical || volume.AhdiLogical)
                throw new ArgumentException("SGI data partitions must leave the volume header reserved and not overlap.");
            _ = Type(volume); end = volume.OffsetBytes + volume.LengthBytes;
        }
    }

    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes) => Write(disk, volumes, new());
    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes, LegacyDiskGeometry geometry)
    {
        Validate(disk.Length, volumes, geometry);
        var header = new byte[512];
        void U16(int at, int value) => BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(at), checked((ushort)value));
        void U32(int at, uint value) => BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(at), value);
        void Partition(int slot, uint start, uint size, uint type)
        { U32(312 + slot * 12, size); U32(316 + slot * 12, start); U32(320 + slot * 12, type); }
        U32(0, 0x0be5a941); U16(4, ushort.MaxValue); U16(6, ushort.MaxValue);
        U16(28, geometry.Validate(disk.Length)); U16(32, geometry.Heads);
        U16(38, geometry.SectorsPerTrack); U16(40, 512); U16(42, 1);
        Partition(8, 0, (uint)(HeaderBytes / 512), 0); Partition(10, 0, (uint)(disk.Length / 512), 6);
        var slots = Enumerable.Range(0, 16).Where(slot => slot is not (8 or 10)).ToArray();
        for (var i = 0; i < volumes.Count; i++)
        {
            var volume = volumes[i];
            Partition(slots[i], (uint)(volume.OffsetBytes / 512), (uint)(volume.LengthBytes / 512), Type(volume));
        }
        uint checksum = 0;
        for (var i = 0; i < 512; i += 4) checksum = unchecked(checksum - BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(i)));
        U32(504, checksum); disk.Position = 0; disk.Write(header); disk.Flush();
    }

    private static uint Type(DiskVolumePlan volume)
    {
        if (uint.TryParse(volume.PartitionType, NumberStyles.None, CultureInfo.InvariantCulture, out var type) && type is not (0 or 6))
            return type;
        if (volume.PartitionType is null && volume.FileSystemId.Equals("none", StringComparison.OrdinalIgnoreCase)) return 3;
        throw new ArgumentException("Provide a decimal SGI type, excluding reserved volume-header and whole-disk types.");
    }
}
