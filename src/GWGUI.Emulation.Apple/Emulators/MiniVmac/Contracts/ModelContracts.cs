using GWGUI.Emulation.Apple.Emulators.MiniVmac.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Contracts;

public sealed record MiniVmacModelDefinition(MiniVmacModel Model, MachineModel MachineModel, long MainMemoryBytes);
