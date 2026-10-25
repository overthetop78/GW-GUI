namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

internal sealed record HardwareModel(MachineModel Model, string CpuName, long CpuFrequencyHz, long MainMemoryBytes, long VideoMemoryBytes, long SaveMemoryBytes);
