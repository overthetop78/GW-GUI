using DiscUtils;
using DiscUtils.Partitions;

namespace GWGUI.Emulation.HardDisks.Partitioning;

public static class MbrPartitionWriter
{
    public static PartitionInfo Create(Stream disk, WellKnownPartitionType type)
    {
        var table = BiosPartitionTable.Initialize(disk, Geometry.FromCapacity(disk.Length));
        table.Create(type, true);
        return table.Partitions.Single();
    }
}
