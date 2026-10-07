using GWGUI.Emulation.Atari.Emulators.Holani.Constants;
using HolaniInputFunctions = GWGUI.Emulation.Atari.Emulators.Holani.Functions.InputFunctions;

namespace GWGUI.Emulation.Atari.Emulators.Holani.Factories;

internal sealed class HolaniMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    public override void PrepareSystemDirectory(MachineConfiguration configuration, string systemDirectory) =>
        FirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, systemDirectory, validateRequired: false);
    public override EmulationInputSnapshot PrepareInput(EmulationInputSnapshot snapshot) =>
        HolaniInputFunctions.ToNative(snapshot);
}
