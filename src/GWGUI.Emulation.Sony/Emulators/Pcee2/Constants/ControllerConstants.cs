namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;

internal static class ControllerConstants
{
    internal const int UsbPortCount = 2;
    internal const int FirstUsbPort = 0;
    internal const int SecondUsbPort = 1;
    internal static IReadOnlyList<int> UsbControllerPorts { get; } = Array.AsReadOnly<int>([FirstUsbPort, SecondUsbPort]);
    internal const string ButtonA = "A";
    internal const string ButtonB = "B";
    internal const string ButtonC = "C";
    internal const string AimLeft = "⊕ ←";
    internal const string AimRight = "⊕ →";
    internal const string AimUp = "⊕ ↑";
    internal const string AimDown = "⊕ ↓";
    internal const string UsbPortPrefix = "USB ";
    internal const string GamepadPortPrefix = "DUALSHOCK 2 · ";
    internal static IReadOnlyList<PeripheralCategory> GamepadTypes { get; } = [PeripheralCategory.Joystick, PeripheralCategory.None];
    internal static IReadOnlyList<PeripheralCategory> DisabledUsbTypes { get; } = [PeripheralCategory.None, PeripheralCategory.GunCon2];
    internal static IReadOnlyList<PeripheralCategory> EnabledUsbTypes { get; } = [PeripheralCategory.GunCon2, PeripheralCategory.None];
}
