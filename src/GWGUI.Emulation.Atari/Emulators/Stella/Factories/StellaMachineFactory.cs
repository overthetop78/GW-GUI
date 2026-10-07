using GWGUI.Emulation.Atari.Emulators.Stella.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Stella.Factories;

internal sealed class StellaMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    internal override bool SupportsCartridgeRegion => EmulatorConstants.SupportsCartridgeRegion;
}
