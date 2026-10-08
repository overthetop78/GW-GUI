using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Factories;

internal sealed class SwanStationMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    protected override MachineConfiguration Configure(MachineConfiguration configuration) =>
        SwanStationOptionFunctions.ToNative(configuration);
}