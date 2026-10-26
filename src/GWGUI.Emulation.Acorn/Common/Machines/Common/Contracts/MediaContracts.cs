namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;

public sealed record MediaConfiguration(string Path, MediaCategory Category, EmulationMediaSlot Slot, string? Label = null, bool IsReadOnly = false, bool IsInserted = true, IReadOnlyDictionary<string, string>? Parameters = null);
