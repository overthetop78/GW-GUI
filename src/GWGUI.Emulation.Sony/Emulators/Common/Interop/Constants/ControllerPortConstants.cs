namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class ControllerPortConstants
{
    internal const uint NoneDevice = 0;
    internal const uint JoypadDevice = 1;
    internal const uint DeviceBaseMask = 0xff;

    internal static IReadOnlyDictionary<ControllerType, IReadOnlyList<string>> NativeNames { get; } =
        new Dictionary<ControllerType, IReadOnlyList<string>>
        {
            [ControllerType.DualShock] = ["DualShock", "Analog Controller (DualShock)"],
            [ControllerType.AnalogController] = ["Analog Controller"],
            [ControllerType.AnalogJoystick] = ["Analog Joystick"],
            [ControllerType.GunCon] = ["Guncon / G-Con 45", "Namco GunCon", "Guncon", "GunCon"],
            [ControllerType.Justifier] = ["Justifier"],
            [ControllerType.Mouse] = ["Mouse", "PlayStation Mouse", "USB Mouse"],
            [ControllerType.Keyboard] = ["USB Keyboard"],
            [ControllerType.KeyboardAndMouse] = ["USB Keyboard + Mouse"],
            [ControllerType.NeGcon] = ["neGcon"],
            [ControllerType.NeGconRumble] = ["neGcon Rumble"]
        };
}
