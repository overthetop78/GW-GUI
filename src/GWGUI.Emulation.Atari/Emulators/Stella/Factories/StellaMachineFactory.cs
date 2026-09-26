using GWGUI.Emulation.Atari.Emulators.Stella.Constants;
using GWGUI.Emulation.Atari.Emulators.Stella.Functions;

namespace GWGUI.Emulation.Atari.Emulators.Stella.Factories;

internal sealed class StellaMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    internal override bool SupportsCartridgeRegion => EmulatorConstants.SupportsCartridgeRegion;

    public override IReadOnlyDictionary<string, string> PrepareOptions(
        IReadOnlyDictionary<string, string> options) => StellaOptionFunctions.ToNative(options);
}
