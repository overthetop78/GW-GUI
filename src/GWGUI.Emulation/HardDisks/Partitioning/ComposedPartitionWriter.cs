using DiscUtils;
using DiscUtils.Partitions;

namespace GWGUI.Emulation.HardDisks.Partitioning;

internal static class ComposedPartitionWriter
{
    internal static void ValidateMbr(long size, IReadOnlyList<DiskVolumePlan> volumes)
    {
        var logical = volumes.Where(v => v.MbrLogical).OrderBy(v => v.OffsetBytes).ToArray();
        var primary = volumes.Where(v => !v.MbrLogical).ToArray();
        if (size / 512 > uint.MaxValue || primary.Length + (logical.Length > 0 ? 1 : 0) > 4 || volumes.Count(v => v.Active) > 1 ||
            volumes.Any(v => v.OffsetBytes < 512 || PartitionTypeDefaults.Mbr(v) is 0 or 5 or 0x0f or 0x85))
            throw new ArgumentException("MBR requires at most four primary entries including the extended partition, and at most one active partition.");
        if (logical.Length == 0) return;
        var start = logical[0].OffsetBytes - 512;
        var end = checked(logical[^1].OffsetBytes + logical[^1].LengthBytes);
        if (start < 512 || logical.Any(v => v.Active) ||
            primary.Any(v => v.OffsetBytes < end && v.OffsetBytes + v.LengthBytes > start))
            throw new ArgumentException("The extended partition must exclude all primary volumes; logical volumes cannot be active.");
        for (var i = 1; i < logical.Length; i++)
            if (logical[i].OffsetBytes - 512 < logical[i - 1].OffsetBytes + logical[i - 1].LengthBytes)
                throw new ArgumentException("Each logical volume requires a reserved EBR sector immediately before its data.");
    }
    internal static void WriteMbr(Stream stream, IReadOnlyList<DiskVolumePlan> volumes)
    {
        var table = BiosPartitionTable.Initialize(stream, Geometry.FromCapacity(stream.Length));
        foreach (var volume in volumes.Where(v => !v.MbrLogical))
            table.CreatePrimaryBySector(volume.OffsetBytes / 512, (volume.OffsetBytes + volume.LengthBytes) / 512 - 1, PartitionTypeDefaults.Mbr(volume), volume.Active);
        var logical = volumes.Where(v => v.MbrLogical).OrderBy(v => v.OffsetBytes).ToArray();
        if (logical.Length == 0) return;
        var first = logical[0].OffsetBytes / 512 - 1;
        var end = (logical[^1].OffsetBytes + logical[^1].LengthBytes) / 512;
        table.CreatePrimaryBySector(first, end - 1, 0x0f, false);
        ExtendedMbrWriter.Write(stream, logical);
    }
    internal static void ValidateGpt(long size, IReadOnlyList<DiskVolumePlan> volumes)
        => GptPartitionWriter.Validate(size, volumes);
    internal static void WriteGpt(Stream stream, IReadOnlyList<DiskVolumePlan> volumes)
        => GptPartitionWriter.Write(stream, volumes);
}
