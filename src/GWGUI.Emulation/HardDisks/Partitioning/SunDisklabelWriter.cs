using System.Buffers.Binary;
using System.Globalization;

namespace GWGUI.Emulation.HardDisks.Partitioning;

public sealed record SunLabelBackupOptions(int AlternateCylinders = 2, int? Head = null);

/// <summary>Big-endian Sun label with VTOC version 1 and eight slices. No boot loader.</summary>
public static class SunDisklabelWriter
{
    public static DiskFormatRegistry.PartitionTable Describe(string id, LegacyDiskGeometry geometry, SunLabelBackupOptions? backups = null)
        => new(id, (size, volumes) => Validate(size, volumes, geometry, backups), (disk, volumes) => Write(disk, volumes, geometry, backups))
        { Identity = new("Sun VTOC", backups is null ? "Big-endian v1, primary label" : "Big-endian v1 with five alternate-cylinder labels") };

    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes) => Validate(capacity, volumes, new());
    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes, LegacyDiskGeometry geometry, SunLabelBackupOptions? backups = null)
    {
        var cylinders = geometry.Validate(capacity);
        if (backups is not null && (backups.AlternateCylinders < 1 || backups.AlternateCylinders > cylinders - 2 ||
            geometry.SectorsPerTrack < 10 || (backups.Head ?? geometry.Heads - 1) < 0 ||
            (backups.Head ?? geometry.Heads - 1) >= geometry.Heads))
            throw new ArgumentException("The backup labels require an alternate cylinder and a track with sectors 1, 3, 5, 7 and 9.");
        var usable = capacity - (backups?.AlternateCylinders ?? 0) * geometry.CylinderBytes;
        if (volumes.Count > 7) throw new ArgumentException("Seven data slices are available; slice 2 describes the disk.");
        long end = geometry.CylinderBytes;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            if (volume.OffsetBytes < end || volume.OffsetBytes % geometry.CylinderBytes != 0 ||
                volume.LengthBytes <= 0 || volume.LengthBytes % 512 != 0 || volume.OffsetBytes > usable ||
                volume.LengthBytes > usable - volume.OffsetBytes || volume.MbrLogical || volume.AhdiLogical ||
                volume.Attributes < 0 || (volume.Attributes & ~0x11L) != 0)
                throw new ArgumentException("Invalid Sun slice boundaries or flags.");
            _ = Type(volume); end = volume.OffsetBytes + volume.LengthBytes;
        }
    }

    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes) => Write(disk, volumes, new());
    public static void Write(Stream disk, IReadOnlyList<DiskVolumePlan> volumes, LegacyDiskGeometry geometry, SunLabelBackupOptions? backups = null)
    {
        Validate(disk.Length, volumes, geometry, backups);
        var physicalCylinders = geometry.Validate(disk.Length);
        var alternateCylinders = backups?.AlternateCylinders ?? 0;
        var usableCylinders = physicalCylinders - alternateCylinders;
        var label = new byte[512];
        void U16(int at, int value) => BinaryPrimitives.WriteUInt16BigEndian(label.AsSpan(at), checked((ushort)value));
        void U32(int at, uint value) => BinaryPrimitives.WriteUInt32BigEndian(label.AsSpan(at), value);
        "GWGUI virtual disk"u8.CopyTo(label);
        U32(128, 1); U16(140, 8); U32(188, 0x600ddeee);
        U16(422, physicalCylinders); U16(430, 1); U16(432, usableCylinders); U16(434, alternateCylinders);
        U16(436, geometry.Heads); U16(438, geometry.SectorsPerTrack);
        U16(142 + 2 * 4, 5); U16(144 + 2 * 4, 1); U32(444 + 2 * 8 + 4, (uint)(usableCylinders * geometry.CylinderBytes / 512));
        for (var i = 0; i < volumes.Count; i++)
        {
            var slot = i < 2 ? i : i + 1; var volume = volumes[i];
            U16(142 + slot * 4, Type(volume)); U16(144 + slot * 4, (int)volume.Attributes);
            U32(444 + slot * 8, (uint)(volume.OffsetBytes / geometry.CylinderBytes));
            U32(448 + slot * 8, (uint)(volume.LengthBytes / 512));
        }
        U16(508, 0xdabe);
        ushort checksum = 0;
        for (var i = 0; i < 510; i += 2) checksum ^= BinaryPrimitives.ReadUInt16BigEndian(label.AsSpan(i));
        U16(510, checksum);
        if (backups is not null)
        {
            var track = ((long)(physicalCylinders - 1) * geometry.Heads + (backups.Head ?? geometry.Heads - 1)) * geometry.SectorsPerTrack;
            for (var sector = 1; sector <= 9; sector += 2)
            { disk.Position = (track + sector) * 512; disk.Write(label); }
        }
        disk.Position = 0; disk.Write(label); disk.Flush();
    }

    private static ushort Type(DiskVolumePlan volume)
    {
        if (ushort.TryParse(volume.PartitionType, NumberStyles.None, CultureInfo.InvariantCulture, out var type) && type != 5)
            return type;
        if (volume.PartitionType is null && volume.FileSystemId.Equals("none", StringComparison.OrdinalIgnoreCase)) return 0;
        throw new ArgumentException("Provide a decimal Sun slice tag, excluding the reserved whole-disk tag 5.");
    }
}
