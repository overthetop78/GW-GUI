namespace GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;

public sealed record MediaConfiguration(
    string Path,
    MediaCategory Category,
    string? Label = null,
    bool IsReadOnly = false,
    bool IsInserted = true,
    int MountOrder = 0);
