using GearOptions = GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants.OptionConstants;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, ExternalHostCallbacks callbacks, MachineConfiguration configuration)
    {
        if (configuration.Core == Emulator.GearColeco)
        {
            var spinners = (configuration.Input.Controllers ?? [])
                .Select(binding => binding.Peripheral switch
                {
                    PeripheralCategory.SuperAction => MediaRuntimeConstants.SuperAction,
                    PeripheralCategory.Roller => MediaRuntimeConstants.Roller,
                    PeripheralCategory.Driving => MediaRuntimeConstants.Wheel,
                    _ => null
                })
                .Where(value => value is not null).Distinct(StringComparer.Ordinal).ToArray();
            if (spinners.Length > 1)
                throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid, ErrorMessages.ContentLoadFailed);
            callbacks.SetOption(GearOptions.OptionSpinners, spinners.FirstOrDefault() ?? MediaRuntimeConstants.Disabled);
        }
        callbacks.ConfigureInput(configuration.Input);
        for (var port = ControllerConstants.FirstPort; port < ControllerConstants.PortCount; port++)
            ConfigurePort(exports, callbacks, configuration, port, configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port)?.Peripheral ?? PeripheralCategory.Standard);
    }

    internal static void ConfigurePort(ExternalCoreExports exports, ExternalHostCallbacks callbacks, MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        if (port is < ControllerConstants.FirstPort or >= ControllerConstants.PortCount)
            throw new ArgumentOutOfRangeException(nameof(port));
        if (!Enum.IsDefined(peripheral)) throw new ArgumentOutOfRangeException(nameof(peripheral));
        exports.SetControllerPortDevice((uint)port, peripheral == PeripheralCategory.None ? CoreLifecycleConstants.NoDevice : configuration.Core == Emulator.GearColeco ? CoreLifecycleConstants.ColecoVisionDevice : CoreLifecycleConstants.DefaultJoypadDevice);
    }
}
