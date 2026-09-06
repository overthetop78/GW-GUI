namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>ext3 with a 4 MiB internal journal, 4 KiB blocks and indirect block addressing.</summary>
public static class Ext3VolumeFormatter
{
    public static void Validate(long capacity, string label) => ExtVolumeFormatter.Validate(capacity, label);
    public static void Format(Stream volume, string label) => ExtVolumeFormatter.Format(volume, label, journal: true, extents: false);
}
