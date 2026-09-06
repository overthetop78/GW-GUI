namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>512-byte sectors with 16-bit cylinder, head and track fields.</summary>
public sealed record LegacyDiskGeometry(int Heads = 16, int SectorsPerTrack = 32)
{
    public long CylinderBytes => (long)Heads * SectorsPerTrack * 512;

    internal ushort Validate(long capacity)
    {
        if (Heads is < 1 or > ushort.MaxValue || SectorsPerTrack is < 1 or > ushort.MaxValue ||
            capacity <= 0 || capacity % CylinderBytes != 0 || capacity / CylinderBytes > ushort.MaxValue ||
            capacity / 512 > uint.MaxValue)
            throw new ArgumentException("Capacity must fit complete cylinders and the disklabel geometry fields.");
        return checked((ushort)(capacity / CylinderBytes));
    }
}
