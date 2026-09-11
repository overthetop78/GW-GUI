namespace GWGUI.Emulation.HardDisks;

public sealed record DiskImagePlan
{
    public DiskImagePlan(
        long capacityBytes,
        string containerId,
        string partitionTableId,
        IReadOnlyList<DiskVolumePlan> volumes,
        bool fixedSize = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacityBytes);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionTableId);
        ArgumentNullException.ThrowIfNull(volumes);
        if (volumes.Any(volume => volume is null))
            throw new ArgumentException("A disk image plan cannot contain a null volume.", nameof(volumes));

        CapacityBytes = capacityBytes;
        ContainerId = containerId;
        PartitionTableId = partitionTableId;
        Volumes = Array.AsReadOnly(volumes.ToArray());
        FixedSize = fixedSize;
    }

    public long CapacityBytes { get; init; }

    public string ContainerId { get; init; }

    public string PartitionTableId { get; init; }

    public IReadOnlyList<DiskVolumePlan> Volumes { get; init; }

    public bool FixedSize { get; init; }

    public void ValidateComposition(DiskFormatRegistry registry, DiskConsumerProfile? consumer = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        DiskImageBuilder.Validate(this, registry, consumer);
    }
}
