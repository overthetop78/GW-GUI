using GWGUI.Emulation.Atari.Emulators.Handy.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Handy.Factories;

internal sealed class HandyMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string> CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    public override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => FirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, systemDirectory, validateRequired: false);
}
