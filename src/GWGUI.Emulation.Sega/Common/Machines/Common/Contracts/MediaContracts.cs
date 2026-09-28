namespace GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;

public sealed record MediaConfiguration(
    string Path,
    MediaCategory Category,
    EmulationMediaSlot Slot = default,
    string? Label = null,
    bool IsReadOnly = false,
    bool IsInserted = true,
    int MountOrder = 0);
