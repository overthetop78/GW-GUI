namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Non-journaled HFSX v5 with binary catalog key comparison.</summary>
public static class HfsxVolumeFormatter
{
    public static void Validate(long capacity, string label) => HfsVolumeWriter.Validate(capacity, label);
    public static void Format(Stream volume, string label) => HfsVolumeWriter.Format(volume, label, caseSensitive: true);
}
