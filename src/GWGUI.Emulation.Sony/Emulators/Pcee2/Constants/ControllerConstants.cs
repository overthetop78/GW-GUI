namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;

internal static class ControllerConstants
{
    internal const int UsbPortCount = 2;
    internal const int FirstUsbPort = 0;
    internal const int SecondUsbPort = 1;
    internal const string ButtonA = "A";
    internal const string ButtonB = "B";
    internal const string ButtonC = "C";
    internal const string AimLeft = "⊕ ←";
    internal const string AimRight = "⊕ →";
    internal const string AimUp = "⊕ ↑";
    internal const string AimDown = "⊕ ↓";
    internal const string UsbPortPrefix = "USB ";
    internal const string GamepadPortPrefix = "DUALSHOCK 2 · ";
    internal static IReadOnlyList<ControllerType> GamepadTypes { get; } = [ControllerType.Joystick, ControllerType.None];
    internal static IReadOnlyList<ControllerType> DisabledUsbTypes { get; } = [ControllerType.None, ControllerType.GunCon2];
    internal static IReadOnlyList<ControllerType> EnabledUsbTypes { get; } = [ControllerType.GunCon2, ControllerType.None];
}
