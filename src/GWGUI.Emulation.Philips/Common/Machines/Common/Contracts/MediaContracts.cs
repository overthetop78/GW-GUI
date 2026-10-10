namespace GWGUI.Emulation.Philips.Common.Machines.Common.Contracts;

public sealed record MediaConfiguration(string Path, MediaCategory Category, EmulationMediaSlot Slot,
    string? Label = null, bool IsReadOnly = true, bool IsInserted = true);
