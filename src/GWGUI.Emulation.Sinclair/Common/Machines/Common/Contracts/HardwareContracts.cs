namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Contracts;

public sealed record HardwareModelDefinition(string CpuResourceKey, long? CpuFrequencyHz, long? MainMemoryBytes, int? DisplayWidth, int? DisplayHeight, long? DisplayMemoryBytes = null);
