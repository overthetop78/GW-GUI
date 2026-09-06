using System.Collections.Frozen;

namespace GWGUI.Emulation.HardDisks;

/// <summary>One supported pairing; separate pairings do not imply support for their Cartesian product.</summary>
public sealed class DiskLayoutSupport
{
    public string ContainerId { get; }
    public string PartitionTableId { get; }
    public IReadOnlySet<string> FileSystemIds { get; }
    public IReadOnlySet<int> SectorSizes { get; }

    public DiskLayoutSupport(string containerId, string partitionTableId, IEnumerable<string> fileSystemIds,
        IEnumerable<int>? sectorSizes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionTableId);
        ArgumentNullException.ThrowIfNull(fileSystemIds);
        var formats = fileSystemIds.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        var sectors = (sectorSizes ?? [512]).ToFrozenSet();
        if (formats.Any(string.IsNullOrWhiteSpace) || sectors.Count == 0 || sectors.Any(value => value <= 0))
            throw new ArgumentException("Invalid supported volume formats or sector sizes.");
        ContainerId = containerId;
        PartitionTableId = partitionTableId;
        FileSystemIds = formats;
        SectorSizes = sectors;
    }

    internal bool Matches(DiskImagePlan plan) =>
        ContainerId.Equals(plan.ContainerId, StringComparison.OrdinalIgnoreCase) &&
        PartitionTableId.Equals(plan.PartitionTableId, StringComparison.OrdinalIgnoreCase) &&
        plan.Volumes.All(volume => FileSystemIds.Contains(volume.FileSystemId) && SectorSizes.Contains(volume.SectorBytes));
}

/// <summary>Consumer restrictions supplement format validation; they never broaden a writer's capabilities.</summary>
public sealed class DiskConsumerProfile
{
    public string Id { get; }
    public long MinimumBytes { get; }
    public long MaximumBytes { get; }
    public long CapacityAlignment { get; }
    public long MaximumVolumeBytes { get; }
    public int MaximumVolumes { get; }
    public IReadOnlyList<DiskLayoutSupport> Layouts { get; }
    private readonly Action<DiskImagePlan>? validateSpecific;

    public DiskConsumerProfile(string id, IEnumerable<DiskLayoutSupport> layouts, long maximumBytes,
        long minimumBytes = 512, long capacityAlignment = 512, long? maximumVolumeBytes = null,
        int maximumVolumes = int.MaxValue, Action<DiskImagePlan>? validateSpecific = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(layouts);
        var combinations = layouts.ToArray();
        var volumeLimit = maximumVolumeBytes ?? maximumBytes;
        if (combinations.Length == 0 || combinations.Any(value => value is null) || minimumBytes <= 0 ||
            maximumBytes < minimumBytes || capacityAlignment <= 0 || volumeLimit <= 0 || maximumVolumes < 0)
            throw new ArgumentException("Invalid consumer capacity or layout restrictions.");
        Id = id;
        MinimumBytes = minimumBytes;
        MaximumBytes = maximumBytes;
        CapacityAlignment = capacityAlignment;
        MaximumVolumeBytes = volumeLimit;
        MaximumVolumes = maximumVolumes;
        Layouts = Array.AsReadOnly(combinations);
        this.validateSpecific = validateSpecific;
    }

    internal void Validate(DiskImagePlan plan)
    {
        if (plan.CapacityBytes < MinimumBytes || plan.CapacityBytes > MaximumBytes || plan.CapacityBytes % CapacityAlignment != 0)
            throw new ArgumentOutOfRangeException(nameof(plan), $"Disk capacity is unsupported by profile {Id}.");
        if (plan.Volumes.Count > MaximumVolumes || plan.Volumes.Any(volume => volume.LengthBytes > MaximumVolumeBytes))
            throw new ArgumentException($"Volume count or capacity is unsupported by profile {Id}.", nameof(plan));
        if (!Layouts.Any(layout => layout.Matches(plan)))
            throw new NotSupportedException($"The container, partition table and volume formats are not supported together by profile {Id}.");
        validateSpecific?.Invoke(plan);
    }
}
