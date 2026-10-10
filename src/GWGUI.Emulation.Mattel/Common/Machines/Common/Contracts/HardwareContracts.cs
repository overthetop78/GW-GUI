namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts;

public sealed record HardwareModelDefinition(string CpuResourceKey,
    long? CpuFrequencyHz, long? MainMemoryBytes, int? DisplayWidth, int? DisplayHeight,
    long? DisplayMemoryBytes = null);
