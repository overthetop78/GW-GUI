namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, MachineConfiguration configuration)
    {
        var count = ConfigurationFunctions.ControllerPortCount(configuration.Model);
        var controllers = configuration.Input.Controllers ?? [];
        var mouseConfigured = false;
        for (var port = ControllerConstants.FirstPort; port < count; port++)
        {
            var peripheral = controllers.FirstOrDefault(controller => controller.Port == port)?.Peripheral ?? PeripheralCategory.None;
            var device = Device(peripheral);
            if (configuration.Core == Emulator.Fuse && device == InputConstants.NoDevice && !mouseConfigured && configuration.Input.Mouse?.Count > 0)
            {
                device = InputConstants.KempstonMouse;
                mouseConfigured = true;
            }
            exports.SetControllerPortDevice(checked((uint)port), device);
        }
        var keyboardPort = configuration.Core == Emulator.EightyOne ? InputConstants.EightyOneKeyboardPort : InputConstants.FuseKeyboardPort;
        exports.SetControllerPortDevice(keyboardPort, InputConstants.SinclairKeyboard);
    }

    private static uint Device(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.CursorJoystick => InputConstants.CursorJoystick,
        PeripheralCategory.KempstonJoystick => InputConstants.KempstonJoystick,
        PeripheralCategory.Sinclair1Joystick => InputConstants.Sinclair1Joystick,
        PeripheralCategory.Sinclair2Joystick => InputConstants.Sinclair2Joystick,
        PeripheralCategory.Timex1Joystick => InputConstants.Timex1Joystick,
        PeripheralCategory.Timex2Joystick => InputConstants.Timex2Joystick,
        PeripheralCategory.FullerJoystick => InputConstants.FullerJoystick,
        _ => InputConstants.NoDevice
    };
}
