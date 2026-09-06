namespace GWGUI.Emulation.HardDisks;

/// <summary>BIOS CHS geometry, independent of a physical device or consumer.</summary>
public sealed record DiskChsGeometry
{
    public int Heads { get; }
    public int SectorsPerTrack { get; }
    public long MaximumBytes => 1024L * Heads * SectorsPerTrack * 512;

    public DiskChsGeometry(int heads = 16, int sectorsPerTrack = 63)
    {
        if (heads is < 1 or > 255) throw new ArgumentOutOfRangeException(nameof(heads));
        if (sectorsPerTrack is < 1 or > 63) throw new ArgumentOutOfRangeException(nameof(sectorsPerTrack));
        Heads = heads; SectorsPerTrack = sectorsPerTrack;
    }

    internal DiscUtils.Geometry ForCapacity(long capacity) => new(
        checked((int)((capacity + Heads * SectorsPerTrack * 512L - 1) / (Heads * SectorsPerTrack * 512L))),
        Heads, SectorsPerTrack, 512);
}
