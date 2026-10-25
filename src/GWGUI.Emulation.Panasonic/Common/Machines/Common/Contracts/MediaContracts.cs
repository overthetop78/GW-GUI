namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

public sealed record MediaConfiguration(string Path, EmulationMediaType Category, EmulationMediaSlot Slot, string? Label = null, bool IsReadOnly = true, bool IsInserted = true);
