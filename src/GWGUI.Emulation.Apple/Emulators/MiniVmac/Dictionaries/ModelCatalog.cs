using GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Contracts;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Dictionaries;

public static class MiniVmacModelCatalog
{
    public static IReadOnlyList<MiniVmacModelDefinition> All { get; } =
    [new(MiniVmacModel.MacintoshII, MachineModel.MacintoshII, EmulatorConstants.MainMemoryBytes)];
}
