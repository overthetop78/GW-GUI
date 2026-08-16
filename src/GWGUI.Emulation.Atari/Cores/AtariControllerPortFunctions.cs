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
                binding?.Peripheral ?? AtariPeripheralKind.Automatic));
        }
    }

    internal static uint ResolveDevice(IReadOnlyList<AtariControllerPort> ports, int port,
        AtariPeripheralKind peripheral)
    {
        if (peripheral == AtariPeripheralKind.None)
            return AtariCoreLifecycleConstants.NoDevice;

        var devices = port < ports.Count ? ports[port].Devices : [];
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
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
        var selected = devices.FirstOrDefault(device =>
            device.Description.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
            return selected.Id;
        if (peripheral == AtariPeripheralKind.Automatic)
            return devices.FirstOrDefault()?.Id ?? AtariCoreLifecycleConstants.DefaultJoypadDevice;
        throw new InvalidDataException(AtariErrorMessages.UnsupportedControllerDevice);
    }
}
