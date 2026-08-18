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
                binding?.Peripheral ?? AtariPeripheralKind.Automatic, configuration.Core));
        }
    }

    internal static uint ResolveDevice(IReadOnlyList<AtariControllerPort> ports, int port,
        AtariPeripheralKind peripheral, AtariCoreKind? core = null)
    {
        if (peripheral == AtariPeripheralKind.None)
            return AtariCoreLifecycleConstants.NoDevice;

        var devices = port < ports.Count ? ports[port].Devices : [];
        if (core == AtariCoreKind.Stella && peripheral is not AtariPeripheralKind.None)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains("Automatic", StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? AtariCoreLifecycleConstants.DefaultJoypadDevice;
        var name = peripheral switch
        {
            AtariPeripheralKind.Automatic => AtariCoreLifecycleConstants.JoypadDeviceName,
            AtariPeripheralKind.Keyboard => AtariCoreLifecycleConstants.KeyboardDeviceName,
            AtariPeripheralKind.Mouse => AtariCoreLifecycleConstants.MouseDeviceName,
            AtariPeripheralKind.Joystick => AtariCoreLifecycleConstants.JoystickDeviceName,
            AtariPeripheralKind.AnalogJoystick => AtariCoreLifecycleConstants.AnalogDeviceName,
            AtariPeripheralKind.Paddle => AtariCoreLifecycleConstants.PaddleDeviceName,
            AtariPeripheralKind.LightGun => AtariCoreLifecycleConstants.LightGunDeviceName,
            AtariPeripheralKind.NumericKeypad => AtariCoreLifecycleConstants.NumericKeypadDeviceName,
            AtariPeripheralKind.DrivingController => AtariCoreLifecycleConstants.DrivingControllerDeviceName,
            AtariPeripheralKind.ProLineController => AtariCoreLifecycleConstants.ProLineControllerDeviceName,
            AtariPeripheralKind.EnhancedController => AtariCoreLifecycleConstants.EnhancedControllerDeviceName,
            AtariPeripheralKind.BoosterGrip => "Booster Grip",
            AtariPeripheralKind.GenesisController => "Genesis",
            AtariPeripheralKind.Joy2BPlus => "Joy 2B+",
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
        var selected = devices.FirstOrDefault(device =>
            device.Description.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
            return selected.Id;
        // Several cores expose the physical emulated controller only as a generic
        // Libretro joypad. The selected semantic type still drives GW GUI's mappings.
        if (peripheral is AtariPeripheralKind.Joystick or AtariPeripheralKind.AnalogJoystick
            or AtariPeripheralKind.Paddle or AtariPeripheralKind.DrivingController
            or AtariPeripheralKind.ProLineController or AtariPeripheralKind.EnhancedController
            or AtariPeripheralKind.LightGun or AtariPeripheralKind.NumericKeypad
            or AtariPeripheralKind.BoosterGrip or AtariPeripheralKind.GenesisController
            or AtariPeripheralKind.Joy2BPlus)
            return devices.FirstOrDefault(device =>
                       device.Description.Contains(AtariCoreLifecycleConstants.JoypadDeviceName,
                           StringComparison.OrdinalIgnoreCase)
                       || device.Description.Contains(AtariCoreLifecycleConstants.JoystickDeviceName,
                           StringComparison.OrdinalIgnoreCase))?.Id
                   ?? devices.FirstOrDefault()?.Id
                   ?? AtariCoreLifecycleConstants.DefaultJoypadDevice;
        if (peripheral == AtariPeripheralKind.Automatic)
            return devices.FirstOrDefault()?.Id ?? AtariCoreLifecycleConstants.DefaultJoypadDevice;
        throw new InvalidDataException(AtariErrorMessages.UnsupportedControllerDevice);
    }

    internal static void ConfigurePort(AtariExternalCoreExports exports, AtariExternalHostCallbacks callbacks,
        AtariMachineConfiguration configuration, int port, AtariPeripheralKind peripheral)
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
