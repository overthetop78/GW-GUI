namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class ControllerPortConstants
{
    internal static IReadOnlyList<int> UsbControllerPorts { get; } = Array.AsReadOnly<int>([8, 9]);
    internal const uint NoneDevice = 0;
    internal const uint JoypadDevice = 1;
    internal const uint DeviceBaseMask = 0xff;

    internal static IReadOnlyDictionary<PeripheralCategory, IReadOnlyList<string>> NativeNames { get; } =
        new Dictionary<PeripheralCategory, IReadOnlyList<string>>
        {
            [PeripheralCategory.DualShock] = ["DualShock", "Analog Controller (DualShock)"],
            [PeripheralCategory.AnalogController] = ["Analog Controller"],
            [PeripheralCategory.AnalogJoystick] = ["Analog Joystick"],
            [PeripheralCategory.GunCon] = ["Guncon / G-Con 45", "Namco GunCon", "Guncon", "GunCon"],
            [PeripheralCategory.Justifier] = ["Justifier"],
            [PeripheralCategory.Mouse] = ["Mouse", "PlayStation Mouse", "USB Mouse"],
            [PeripheralCategory.Keyboard] = ["USB Keyboard"],
            [PeripheralCategory.KeyboardAndMouse] = ["USB Keyboard + Mouse"],
            [PeripheralCategory.NeGcon] = ["neGcon"],
            [PeripheralCategory.NeGconRumble] = ["neGcon Rumble"]
        };
}
