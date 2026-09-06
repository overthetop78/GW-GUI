using DiscUtils;
using DiscUtils.Fat;

namespace GWGUI.Emulation.HardDisks.FileSystems;

public static class FatVolumeFormatter
{
    public static void Format(Stream volume, string label, long firstSector = 0, DiskChsGeometry? geometry = null)
    {
        using var fs = FatFileSystem.FormatPartition(volume, label,
            geometry?.ForCapacity(volume.Length) ?? Geometry.FromCapacity(volume.Length), checked((int)firstSector), checked((int)(volume.Length / 512)), 1);
    }
}
