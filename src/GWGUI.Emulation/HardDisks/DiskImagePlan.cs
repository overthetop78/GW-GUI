namespace GWGUI.Emulation.HardDisks;

public sealed record DiskImagePlan(long CapacityBytes, string ContainerId, string PartitionTableId,
    IReadOnlyList<DiskVolumePlan> Volumes, bool FixedSize = false);
