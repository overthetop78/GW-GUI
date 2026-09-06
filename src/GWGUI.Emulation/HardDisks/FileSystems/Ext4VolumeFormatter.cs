namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>ext4 with extents and a 4 MiB internal journal. No 64-bit or metadata checksum features.</summary>
public static class Ext4VolumeFormatter
{
    public static void Validate(long capacity, string label) => ExtVolumeFormatter.Validate(capacity, label);
    public static void Format(Stream volume, string label) => ExtVolumeFormatter.Format(volume, label, journal: true, extents: true);
}
