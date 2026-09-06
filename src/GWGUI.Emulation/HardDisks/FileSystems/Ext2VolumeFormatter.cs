namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>ext2 dynamic revision, 4 KiB blocks, 128-byte inodes, no journal.</summary>
public static class Ext2VolumeFormatter
{
    public static void Validate(long capacity, string label) => ExtVolumeFormatter.Validate(capacity, label);
    public static void Format(Stream volume, string label) => ExtVolumeFormatter.Format(volume, label, journal: false, extents: false);
}
