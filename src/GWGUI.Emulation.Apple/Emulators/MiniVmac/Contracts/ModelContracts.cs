using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Enums;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Contracts;

public sealed record MiniVmacModelDefinition(MiniVmacModel Model, MachineModel MachineModel, long MainMemoryBytes);
