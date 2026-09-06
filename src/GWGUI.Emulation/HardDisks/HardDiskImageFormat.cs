namespace GWGUI.Emulation.HardDisks;

/// <summary>A machine's supported combination of container, attachment and capacity policy.</summary>
public sealed record HardDiskImageFormat(string Id, string Extension, string InterfaceName,
    long MaximumBytes, long DefaultBytes, IReadOnlyList<HardDiskPreparation>? Preparations = null,
    Containers.DiskContainerKind Container = Containers.DiskContainerKind.Raw)
{
    public string DisplayName => $"{InterfaceName} (*{Extension})";
}
