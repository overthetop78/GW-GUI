namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;

internal sealed record HardwareModel(MachineModel Model, string CpuName, long CpuFrequencyHz, long MainMemoryBytes);
