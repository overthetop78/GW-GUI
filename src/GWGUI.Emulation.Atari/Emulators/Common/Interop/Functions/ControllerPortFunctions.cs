namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration)
    {
        var portCount = CompatibilityCatalog.Get(configuration.Model).ControllerPortCount;
        for (var port = ControllerPortConstants.MinimumControllerPort; port < portCount; port++)
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            exports.SetControllerPortDevice((uint)port, ResolveDevice(callbacks.ControllerPorts, port,
                binding?.Peripheral ?? PeripheralCategory.Automatic, configuration.Core));
        }
    }

    internal static uint ResolveDevice(IReadOnlyList<ControllerPort> ports, int port,
        PeripheralCategory peripheral, Emulator? core = null)
    {
        if (peripheral == PeripheralCategory.None)
            return CoreLifecycleConstants.NoDevice;

        var devices = port < ports.Count ? ports[port].Devices : [];
        if (core is Emulator.Stella2023 or Emulator.Stella or Emulator.Stella2014 && peripheral is not PeripheralCategory.None)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains(ControllerPortFunctionsConstants.Automatic, StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? CoreLifecycleConstants.DefaultJoypadDevice;
        var name = peripheral switch
        {
            PeripheralCategory.Automatic => CoreLifecycleConstants.JoypadDeviceName,
            PeripheralCategory.Keyboard => CoreLifecycleConstants.KeyboardDeviceName,
            PeripheralCategory.Mouse => CoreLifecycleConstants.MouseDeviceName,
            PeripheralCategory.Joystick => CoreLifecycleConstants.JoystickDeviceName,
            PeripheralCategory.AnalogJoystick => CoreLifecycleConstants.AnalogDeviceName,
            PeripheralCategory.Paddle => CoreLifecycleConstants.PaddleDeviceName,
            PeripheralCategory.LightGun => CoreLifecycleConstants.LightGunDeviceName,
            PeripheralCategory.NumericKeypad => CoreLifecycleConstants.NumericKeypadDeviceName,
            PeripheralCategory.DrivingController => CoreLifecycleConstants.DrivingControllerDeviceName,
            PeripheralCategory.ProLineController => CoreLifecycleConstants.ProLineControllerDeviceName,
            PeripheralCategory.EnhancedController => CoreLifecycleConstants.EnhancedControllerDeviceName,
            PeripheralCategory.BoosterGrip => ControllerDeviceConstants.BoosterGrip,
            PeripheralCategory.GenesisController => ControllerDeviceConstants.Genesis,
            PeripheralCategory.Joy2BPlus => ControllerDeviceConstants.Joy2B,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
        var selected = devices.FirstOrDefault(device =>
            device.Description.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
            return selected.Id;
        // Several cores expose the physical emulated controller only as a generic
        // Libretro joypad. The selected semantic type still drives GW GUI's mappings.
        if (peripheral is PeripheralCategory.Joystick or PeripheralCategory.AnalogJoystick
            or PeripheralCategory.Paddle or PeripheralCategory.DrivingController
            or PeripheralCategory.ProLineController or PeripheralCategory.EnhancedController
            or PeripheralCategory.LightGun or PeripheralCategory.NumericKeypad
            or PeripheralCategory.BoosterGrip or PeripheralCategory.GenesisController
            or PeripheralCategory.Joy2BPlus)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains(CoreLifecycleConstants.JoypadDeviceName,
                           StringComparison.OrdinalIgnoreCase)
                       || device.Description.Contains(CoreLifecycleConstants.JoystickDeviceName,
                           StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? CoreLifecycleConstants.DefaultJoypadDevice;
        if (peripheral == PeripheralCategory.Automatic)
            return devices.FirstOrDefault()?.Id ?? CoreLifecycleConstants.DefaultJoypadDevice;
        throw new InvalidDataException(ErrorMessages.UnsupportedControllerDevice);
    }

    internal static void ConfigurePort(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        var definition = CompatibilityCatalog.Get(configuration.Model);
        if (port < ControllerPortConstants.MinimumControllerPort || port >= definition.ControllerPortCount)
            throw new ArgumentOutOfRangeException(nameof(port), ErrorMessages.InvalidControllerPort);
        if (!ControllerFunctions.Peripherals(configuration.Model).Contains(peripheral))
            throw new InvalidDataException(ErrorMessages.UnsupportedControllerDevice);
        exports.SetControllerPortDevice(checked((uint)port), ResolveDevice(callbacks.ControllerPorts, port,
            peripheral, configuration.Core));
    }
}
