namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariControllerPortFunctions
{
    internal static void Configure(AtariExternalCoreExports exports, AtariExternalHostCallbacks callbacks,
        AtariMachineConfiguration configuration)
    {
        var portCount = AtariCompatibilityCatalog.Get(configuration.Model).ControllerPortCount;
        for (var port = AtariConstants.MinimumControllerPort; port < portCount; port++)
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            exports.SetControllerPortDevice((uint)port, ResolveDevice(callbacks.ControllerPorts, port,
                binding?.Peripheral ?? AtariPeripheralCategory.Automatic, configuration.Core));
        }
    }

    internal static uint ResolveDevice(IReadOnlyList<AtariControllerPort> ports, int port,
        AtariPeripheralCategory peripheral, AtariEmulator? core = null)
    {
        if (peripheral == AtariPeripheralCategory.None)
            return AtariCoreLifecycleConstants.NoDevice;

        var devices = port < ports.Count ? ports[port].Devices : [];
        if (core == AtariEmulator.Stella && peripheral is not AtariPeripheralCategory.None)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains("Automatic", StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? AtariCoreLifecycleConstants.DefaultJoypadDevice;
        var name = peripheral switch
        {
            AtariPeripheralCategory.Automatic => AtariCoreLifecycleConstants.JoypadDeviceName,
            AtariPeripheralCategory.Keyboard => AtariCoreLifecycleConstants.KeyboardDeviceName,
            AtariPeripheralCategory.Mouse => AtariCoreLifecycleConstants.MouseDeviceName,
            AtariPeripheralCategory.Joystick => AtariCoreLifecycleConstants.JoystickDeviceName,
            AtariPeripheralCategory.AnalogJoystick => AtariCoreLifecycleConstants.AnalogDeviceName,
            AtariPeripheralCategory.Paddle => AtariCoreLifecycleConstants.PaddleDeviceName,
            AtariPeripheralCategory.LightGun => AtariCoreLifecycleConstants.LightGunDeviceName,
            AtariPeripheralCategory.NumericKeypad => AtariCoreLifecycleConstants.NumericKeypadDeviceName,
            AtariPeripheralCategory.DrivingController => AtariCoreLifecycleConstants.DrivingControllerDeviceName,
            AtariPeripheralCategory.ProLineController => AtariCoreLifecycleConstants.ProLineControllerDeviceName,
            AtariPeripheralCategory.EnhancedController => AtariCoreLifecycleConstants.EnhancedControllerDeviceName,
            AtariPeripheralCategory.BoosterGrip => "Booster Grip",
            AtariPeripheralCategory.GenesisController => "Genesis",
            AtariPeripheralCategory.Joy2BPlus => "Joy 2B+",
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
        var selected = devices.FirstOrDefault(device =>
            device.Description.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
            return selected.Id;
        // Several cores expose the physical emulated controller only as a generic
        // Libretro joypad. The selected semantic type still drives GW GUI's mappings.
        if (peripheral is AtariPeripheralCategory.Joystick or AtariPeripheralCategory.AnalogJoystick
            or AtariPeripheralCategory.Paddle or AtariPeripheralCategory.DrivingController
            or AtariPeripheralCategory.ProLineController or AtariPeripheralCategory.EnhancedController
            or AtariPeripheralCategory.LightGun or AtariPeripheralCategory.NumericKeypad
            or AtariPeripheralCategory.BoosterGrip or AtariPeripheralCategory.GenesisController
            or AtariPeripheralCategory.Joy2BPlus)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains(AtariCoreLifecycleConstants.JoypadDeviceName,
                           StringComparison.OrdinalIgnoreCase)
                       || device.Description.Contains(AtariCoreLifecycleConstants.JoystickDeviceName,
                           StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? AtariCoreLifecycleConstants.DefaultJoypadDevice;
        if (peripheral == AtariPeripheralCategory.Automatic)
            return devices.FirstOrDefault()?.Id ?? AtariCoreLifecycleConstants.DefaultJoypadDevice;
        throw new InvalidDataException(AtariErrorMessages.UnsupportedControllerDevice);
    }

    internal static void ConfigurePort(AtariExternalCoreExports exports, AtariExternalHostCallbacks callbacks,
        AtariMachineConfiguration configuration, int port, AtariPeripheralCategory peripheral)
    {
        var definition = AtariCompatibilityCatalog.Get(configuration.Model);
        if (port < AtariConstants.MinimumControllerPort || port >= definition.ControllerPortCount)
            throw new ArgumentOutOfRangeException(nameof(port), AtariErrorMessages.InvalidControllerPort);
        if (!AtariControllerFunctions.Peripherals(configuration.Model).Contains(peripheral))
            throw new InvalidDataException(AtariErrorMessages.UnsupportedControllerDevice);
        exports.SetControllerPortDevice(checked((uint)port), ResolveDevice(callbacks.ControllerPorts, port,
            peripheral, configuration.Core));
    }
}
