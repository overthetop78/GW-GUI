namespace GWGUI.Emulation.HardDisks;

public static class HardDiskPath
{
    public static bool Equals(string first, string second) => string.Equals(
        Path.GetFullPath(first), Path.GetFullPath(second),
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}
