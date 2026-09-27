using GWGUI.Emulation.Atari.Emulators.BeetleLynx.Constants;

namespace GWGUI.Emulation.Atari.Emulators.BeetleLynx.Factories;

internal sealed class BeetleLynxMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
}
