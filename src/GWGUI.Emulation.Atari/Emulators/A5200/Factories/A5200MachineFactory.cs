using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Emulators.A5200.Constants;
using InputConstants = GWGUI.Emulation.Atari.Emulators.A5200.Constants.InputConstants;
using InputFunctions = GWGUI.Emulation.Atari.Emulators.A5200.Functions.InputFunctions;

namespace GWGUI.Emulation.Atari.Emulators.A5200.Factories;

internal sealed class A5200MachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    internal override IReadOnlySet<string>? CartridgeExtensions => EmulatorConstants.CartridgeExtensions;
    internal override bool SupportsCartridgeRegion => EmulatorConstants.SupportsCartridgeRegion;

    public override IReadOnlyDictionary<string, string> GetConfiguredOptions(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        options.TryAdd(OptionConstants.BiosOption, configuration.Firmwares.Any(
            firmware => firmware.Category == FirmwareCategory.Atari5200Bios)
            ? OptionConstants.OfficialBios : OptionConstants.InternalBios);
        return options;
    }

    public override EmulationInputSnapshot PrepareInput(EmulationInputSnapshot snapshot) =>
        InputFunctions.ToNative(snapshot);

    public override void ConfigureController(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        if (port < InputConstants.PrimaryPort || port >= Atari5200ModelConstants.FourPorts)
            throw new ArgumentOutOfRangeException(nameof(port));
        if (!ControllerFunctions.Peripherals(configuration.Model).Contains(peripheral))
            throw new ArgumentOutOfRangeException(nameof(peripheral));
        if (port >= InputConstants.SupportedPortCount) return;
        exports.SetControllerPortDevice(checked((uint)port), port == InputConstants.PrimaryPort
            ? InputConstants.DirectKeypadDevice : CoreLifecycleConstants.DefaultJoypadDevice);
    }
}
