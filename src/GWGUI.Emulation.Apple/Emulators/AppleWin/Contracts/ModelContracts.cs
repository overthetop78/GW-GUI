using GWGUI.Emulation.Apple.Emulators.AppleWin.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Contracts;

public sealed record AppleWinModelDefinition(AppleWinModel Model, MachineModel MachineModel, string NativeValue);
