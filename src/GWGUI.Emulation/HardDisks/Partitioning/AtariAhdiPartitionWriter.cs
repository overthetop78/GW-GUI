using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Partitioning;

public static class AtariAhdiPartitionWriter
{
    public static Stream Create(Stream disk)
    {
        if (disk.Length < 1024 * 1024) throw new ArgumentOutOfRangeException(nameof(disk));
        AhdiPartitionWriter.Write(disk, [new(512, disk.Length - 512)]);
        return new SubStream(disk, Ownership.None, 512, disk.Length - 512);
    }
}
