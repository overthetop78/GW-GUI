using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks;

public static class DiskImageBuilder
{
    public static void Validate(DiskImagePlan plan, DiskFormatRegistry? registry = null, DiskConsumerProfile? consumer = null)
        => _ = Prepare(plan, registry, consumer);

    public static void Write(Stream destination, DiskImagePlan plan, DiskFormatRegistry? registry = null, DiskConsumerProfile? consumer = null)
        => Prepare(plan, registry, consumer)(destination);

    internal static Action<Stream> Prepare(DiskImagePlan plan, DiskFormatRegistry? registry = null, DiskConsumerProfile? consumer = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        registry ??= DiskFormatRegistry.CreateDefault();
        var container = registry.GetContainer(plan.ContainerId);
        var initialize = PrepareContent(plan, registry, consumer, container.Validate, container.LogicalSectorSizes);
        return destination =>
        {
            Containers.ContainerValidation.Validate(destination, plan.CapacityBytes);
            container.Write(destination, plan.CapacityBytes, plan.FixedSize, initialize);
        };
    }

    public static void ValidateSet(DiskImagePlan plan, DiskFormatRegistry? registry = null, DiskConsumerProfile? consumer = null)
        => _ = PrepareSet(plan, registry, consumer);

    public static void WriteSet(DiskImagePlan plan, Action<string, Stream> emit,
        DiskFormatRegistry? registry = null, DiskConsumerProfile? consumer = null)
    {
        ArgumentNullException.ThrowIfNull(emit);
        PrepareSet(plan, registry, consumer)(emit);
    }

    /// <summary>Validates first, then publishes all members together in a new directory.</summary>
    public static string CreateSet(string directory, DiskImagePlan plan,
        DiskFormatRegistry? registry = null, DiskConsumerProfile? consumer = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        registry ??= DiskFormatRegistry.CreateDefault();
        var build = PrepareSet(plan, registry, consumer);
        return DiskImageSetPublication.CreateTree(directory, registry.GetImageSet(plan.ContainerId).EntryPoint, build);
    }

    private static Action<Action<string, Stream>> PrepareSet(DiskImagePlan plan, DiskFormatRegistry? registry,
        DiskConsumerProfile? consumer)
    {
        ArgumentNullException.ThrowIfNull(plan);
        registry ??= DiskFormatRegistry.CreateDefault();
        var container = registry.GetImageSet(plan.ContainerId);
        var initialize = PrepareContent(plan, registry, consumer, container.Validate, container.LogicalSectorSizes);
        return emit => container.Write(plan.CapacityBytes, emit, initialize);
    }

    private static Action<Stream> PrepareContent(DiskImagePlan plan, DiskFormatRegistry registry,
        DiskConsumerProfile? consumer, Action<long, bool> validateContainer, IReadOnlySet<int>? containerSectors)
    {
        // Freeze the collection before validation, so callbacks cannot change the planned layout.
        var table = registry.GetPartitionTable(plan.PartitionTableId);
        var volumes = plan.Volumes.Select(volume =>
        {
            if (table.BiosGeometry is null) return volume;
            if (volume.BiosGeometry is not null && volume.BiosGeometry != table.BiosGeometry)
                throw new ArgumentException("The volume geometry disagrees with the partition table.");
            return volume with { BiosGeometry = table.BiosGeometry };
        }).ToArray();
        plan = plan with { Volumes = Array.AsReadOnly(volumes) };
        if (volumes.Any(v => v.MbrLogical) && !table.SupportsMbrLogical)
            throw new ArgumentException("Logical MBR volumes require an MBR partition table.");
        if (volumes.Any(v => v.AhdiLogical) && !plan.PartitionTableId.Equals("ahdi", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Logical AHDI volumes require an AHDI partition table.");
        var formatters = volumes.Select(v => registry.GetFileSystem(v.FileSystemId)).ToArray();
        validateContainer(plan.CapacityBytes, plan.FixedSize);
        if (table.LogicalSectorBytes != 0 &&
            (!(containerSectors?.Contains(table.LogicalSectorBytes) ?? table.LogicalSectorBytes == 512) ||
             volumes.Any(volume => !volume.FileSystemId.Equals("none", StringComparison.OrdinalIgnoreCase) && volume.SectorBytes < table.LogicalSectorBytes)))
            throw new ArgumentException("The partition table logical sectors are unsupported by the container or volume.");
        long end = 0;
        foreach (var volume in volumes.OrderBy(v => v.OffsetBytes))
        {
            if (volume.OffsetBytes < end || volume.LengthBytes <= 0 || volume.OffsetBytes < 0 ||
                volume.OffsetBytes % 512 != 0 || volume.LengthBytes % 512 != 0 ||
                volume.OffsetBytes > plan.CapacityBytes || volume.LengthBytes > plan.CapacityBytes - volume.OffsetBytes)
                throw new ArgumentException("Volumes must be aligned, non-overlapping and inside the disk.");
            end = checked(volume.OffsetBytes + volume.LengthBytes);
        }
        table.Validate(plan.CapacityBytes, volumes);
        for (var i = 0; i < volumes.Length; i++)
        {
            var volume=volumes[i];
            if(volume.SectorBytes<=0 || !(formatters[i].SectorSizes?.Contains(volume.SectorBytes) ?? volume.SectorBytes==512) ||
                volume.OffsetBytes%volume.SectorBytes!=0 || volume.LengthBytes%volume.SectorBytes!=0)
                throw new ArgumentException("The volume sector size is unsupported or its boundaries are not aligned.");
            if (!volume.FileSystemId.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                !(containerSectors?.Any(sector => volume.SectorBytes >= sector && volume.SectorBytes % sector == 0)
                    ?? (volume.SectorBytes >= 512 && volume.SectorBytes % 512 == 0)))
                throw new ArgumentException("The filesystem sectors are incompatible with the container's logical sectors.");
            formatters[i].Validate(volume);
        }
        consumer?.Validate(plan);
        return content =>
        {
            table.Write(content, volumes);
            for (var i = 0; i < volumes.Length; i++)
            {
                using var volume = new SubStream(content, Ownership.None, volumes[i].OffsetBytes, volumes[i].LengthBytes);
                formatters[i].Format(volume, volumes[i]);
            }
        };
    }
}
