using GWGUI.Emulation.Atari.Emulators.Stella2023.Constants;
using GWGUI.Emulation.Atari.Emulators.Stella2023.Functions;

namespace GWGUI.Emulation.Atari.Emulators.Stella2023.Factories;

internal sealed class Stella2023MachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    internal override bool SupportsCartridgeRegion => EmulatorConstants.SupportsCartridgeRegion;

    public override IReadOnlyDictionary<string, string> PrepareOptions(
        IReadOnlyDictionary<string, string> options) => Stella2023OptionFunctions.ToNative(options);
}
