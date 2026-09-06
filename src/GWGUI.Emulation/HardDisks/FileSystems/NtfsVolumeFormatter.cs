using DiscUtils;
using DiscUtils.Ntfs;

namespace GWGUI.Emulation.HardDisks.FileSystems;

public static class NtfsVolumeFormatter
{
    public static void Format(Stream volume, string label, long firstSector = 0, DiskChsGeometry? geometry = null)
    {
        using var fs = NtfsFileSystem.Format(volume, label, geometry?.ForCapacity(volume.Length) ?? Geometry.FromCapacity(volume.Length), firstSector, volume.Length / 512);
    }
}
