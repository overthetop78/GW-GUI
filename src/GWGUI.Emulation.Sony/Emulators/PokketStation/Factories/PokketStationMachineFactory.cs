using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;

namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Factories;

internal sealed class PokketStationMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
