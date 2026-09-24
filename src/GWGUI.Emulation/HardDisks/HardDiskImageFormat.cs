using System.Collections.Frozen;

namespace GWGUI.Emulation.HardDisks;

/// <summary>A machine's supported combination of container, attachment and capacity policy.</summary>
public sealed record HardDiskImageFormat(string Id, string Extension, string InterfaceName,
    long MaximumBytes, long DefaultBytes, IReadOnlyList<HardDiskPreparation>? Preparations = null,
    Containers.DiskContainerKind Container = Containers.DiskContainerKind.Raw)
{
    public long MinimumBytes { get; init; } = 512;

    public IReadOnlySet<int> LogicalSectorSizes { get; init; } = new[] { 512 }.ToFrozenSet();

    public DiskChsGeometry? Geometry { get; init; }

    public DiskFormatOperations Operations { get; init; } = DiskFormatOperations.Create | DiskFormatOperations.Read;

    public bool SupportsFixedAllocation { get; init; } = true;

    public IReadOnlyList<HardDiskPreparation> SupportedPreparations => Preparations ?? [HardDiskPreparation.Blank];

    public string DisplayName => $"{InterfaceName} (*{Extension})";
}
