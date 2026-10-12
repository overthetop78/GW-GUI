using GWGUI.Emulation.Apple.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Interop;
using MiniVmacConstants = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.EmulatorConstants;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration)
    {
        if (configuration.Core == Emulator.MiniVmac)
            exports.SetControllerPortDevice(InputConstants.PrimaryPort, MiniVmacConstants.KeyboardDevice);
        else
            for (var port = ControllerPortConstants.MinimumControllerPort;
                 port < ConfigurationFunctions.ControllerPortCount(configuration.Model); port++)
                ConfigurePort(exports, callbacks, configuration, port,
                    configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port)?.Peripheral
                    ?? PeripheralCategory.Automatic);
        callbacks.ConfigureInput(configuration.Input);
    }

    internal static void ConfigurePort(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        if (port < ControllerPortConstants.MinimumControllerPort
            || port >= ConfigurationFunctions.ControllerPortCount(configuration.Model))
            throw new ArgumentOutOfRangeException(nameof(port), ErrorMessages.InvalidControllerPort);
        if (!ControllerFunctions.Peripherals(configuration.Model).Contains(peripheral))
            throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(peripheral));
        var device = peripheral switch
        {
            PeripheralCategory.None => CoreLifecycleConstants.NoDevice,
            PeripheralCategory.Mouse => InputConstants.MouseDevice,
            PeripheralCategory.Keyboard => InputConstants.KeyboardDevice,
            PeripheralCategory.Automatic or PeripheralCategory.AnalogJoystick => InputConstants.AnalogDevice,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
        exports.SetControllerPortDevice(checked((uint)port), device);
    }
}
