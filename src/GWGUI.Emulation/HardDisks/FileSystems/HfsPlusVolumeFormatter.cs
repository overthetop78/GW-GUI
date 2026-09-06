namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Standalone, non-journaled HFS+ v4 with 4 KiB allocation blocks.</summary>
public static class HfsPlusVolumeFormatter
{
    public static void Validate(long capacity, string label) => HfsVolumeWriter.Validate(capacity, label);
    public static void Format(Stream volume, string label) => HfsVolumeWriter.Format(volume, label, caseSensitive: false);
}
