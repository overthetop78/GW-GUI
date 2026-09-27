using GWGUI.Emulation.Atari.Emulators.ProSystem.Constants;

namespace GWGUI.Emulation.Atari.Emulators.ProSystem.Factories;

internal sealed class ProSystemMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
}
