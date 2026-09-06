using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Partitioning;

public static class AmigaRdbPartitionWriter
{
    public static Stream Create(Stream disk, bool fastFileSystem = true)
    {
        const int cylinderBytes = 32 * 512;
        RdbPartitionWriter.Write(disk, [new(cylinderBytes, disk.Length - cylinderBytes,
            fastFileSystem ? "ffs" : "ofs", "DH0", Active: true)]);
        return new SubStream(disk, Ownership.None, cylinderBytes, disk.Length - cylinderBytes);
    }
}
