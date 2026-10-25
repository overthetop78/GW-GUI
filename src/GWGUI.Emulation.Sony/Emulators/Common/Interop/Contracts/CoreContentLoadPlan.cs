namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

internal sealed record CoreContentLoadPlan(uint SubsystemId, IReadOnlyList<string> Paths, IReadOnlyList<CoreSaveMemoryRegion> SaveMemoryRegions);

internal sealed record CoreSaveMemoryRegion(uint Id, string FileName);
