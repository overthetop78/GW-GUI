using GWGUI.Emulation.Atari.Emulators.Stella2014.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Stella2014.Factories;

internal sealed class Stella2014MachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    internal override bool SupportsCartridgeRegion => EmulatorConstants.SupportsCartridgeRegion;
}
