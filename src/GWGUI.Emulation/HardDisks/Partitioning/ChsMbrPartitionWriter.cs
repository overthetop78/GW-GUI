using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>CHS-addressable MBR with type 05 extended partitions and a reserved track per logical volume.</summary>
public static class ChsMbrPartitionWriter
{
    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes, DiskChsGeometry? geometry = null)
    {
        geometry ??= new();
        if (capacity < 512 || capacity % 512 != 0 || capacity > geometry.MaximumBytes)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        var track = geometry.SectorsPerTrack * 512L;
        var primary = volumes.Where(v => !v.MbrLogical).ToArray();
        var logical = volumes.Where(v => v.MbrLogical).OrderBy(v => v.OffsetBytes).ToArray();
        if (primary.Length + (logical.Length > 0 ? 1 : 0) > 4 || volumes.Count(v => v.Active) > 1 || logical.Any(v => v.Active))
            throw new ArgumentException("Invalid primary or active partition count.");
        long end = track;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            var reservedStart = volume.OffsetBytes - (volume.MbrLogical ? track : 0);
            if (volume.OffsetBytes % track != 0 || volume.LengthBytes <= 0 || volume.LengthBytes % 512 != 0 ||
                reservedStart < end || volume.OffsetBytes > capacity || volume.LengthBytes > capacity - volume.OffsetBytes ||
                volume.AhdiLogical || volume.SectorBytes != 512 || (volume.BiosGeometry is not null && volume.BiosGeometry != geometry))
                throw new ArgumentException("CHS partitions require track-aligned starts and disjoint data and reserved tracks inside the disk.");
            end = volume.OffsetBytes + volume.LengthBytes;
            if (Type(volume) is 0 or 5 or 0x0f or 0x85 or 0x0c or 0x0e)
                throw new ArgumentException("This CHS profile requires a non-extended, non-LBA data partition type.");
        }
        if (logical.Length > 0)
        {
            var first = logical[0].OffsetBytes - track;
            var last = logical[^1].OffsetBytes + logical[^1].LengthBytes;
            if (primary.Any(v => v.OffsetBytes < last && v.OffsetBytes + v.LengthBytes > first))
                throw new ArgumentException("A primary partition overlaps the extended partition.");
        }
    }

    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes, DiskChsGeometry? geometry = null)
    {
        geometry ??= new();
        if (!disk.CanSeek || !disk.CanWrite) throw new ArgumentException("A writable seekable disk is required.");
        Validate(disk.Length, volumes, geometry);
        var track = geometry.SectorsPerTrack;
        var root = new byte[512]; var slot = 0;
        foreach (var volume in volumes.Where(v => !v.MbrLogical))
            Entry(root, slot++, Type(volume), volume.OffsetBytes / 512, volume.LengthBytes / 512, 0, volume.Active);
        var logical = volumes.Where(v => v.MbrLogical).OrderBy(v => v.OffsetBytes).ToArray();
        if (logical.Length > 0)
        {
            var first = logical[0].OffsetBytes / 512 - track;
            var end = (logical[^1].OffsetBytes + logical[^1].LengthBytes) / 512;
            Entry(root, slot, 5, first, end - first, 0, false);
            for (var index = 0; index < logical.Length; index++)
            {
                var volume = logical[index]; var ebr = volume.OffsetBytes / 512 - track;
                var sector = new byte[512];
                Entry(sector, 0, Type(volume), volume.OffsetBytes / 512, volume.LengthBytes / 512, ebr, false);
                if (index + 1 < logical.Length)
                {
                    var next = logical[index + 1]; var nextEbr = next.OffsetBytes / 512 - track;
                    Entry(sector, 1, 5, nextEbr, track + next.LengthBytes / 512, first, false);
                }
                WriteSector(ebr, sector);
            }
        }
        WriteSector(0, root);
        void WriteSector(long lba, byte[] sector)
        {
            sector[510] = 0x55; sector[511] = 0xaa;
            disk.Position = lba * 512; disk.Write(sector);
        }
        void Entry(byte[] sector, int index, byte type, long start, long length, long relativeTo, bool active)
        {
            var entry = sector.AsSpan(446 + index * 16, 16);
            entry[0] = active ? (byte)0x80 : (byte)0; entry[4] = type;
            Chs(entry[1..], start); Chs(entry[5..], start + length - 1);
            BinaryPrimitives.WriteUInt32LittleEndian(entry[8..], checked((uint)(start - relativeTo)));
            BinaryPrimitives.WriteUInt32LittleEndian(entry[12..], checked((uint)length));
        }
        void Chs(Span<byte> bytes, long lba)
        {
            var cylinder = lba / (geometry.Heads * track);
            bytes[0] = (byte)(lba / track % geometry.Heads);
            bytes[1] = (byte)((lba % track + 1) | ((cylinder >> 2) & 0xc0));
            bytes[2] = (byte)cylinder;
        }
    }

    private static byte Type(DiskVolumePlan volume) => volume.MbrType is null && volume.FileSystemId.Equals("fat32", StringComparison.OrdinalIgnoreCase)
        ? (byte)0x0b : PartitionTypeDefaults.Mbr(volume);
}
