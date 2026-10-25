namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

internal enum MouseInput : uint
{
    X, Y, Left, Right, WheelUp, WheelDown, Middle,
    HorizontalWheelUp, HorizontalWheelDown, Button4, Button5
}

internal enum LightGunInput : uint
{
    RelativeX = 0, RelativeY = 1, Trigger = 2, AuxiliaryA = 3, AuxiliaryB = 4,
    Pause = 5, Start = 6, Select = 7, AuxiliaryC = 8,
    Up = 9, Down = 10, Left = 11, Right = 12,
    ScreenX = 13, ScreenY = 14, Offscreen = 15, Reload = 16
}

internal enum AnalogInputIndex : uint { LeftStick, RightStick, Button }
internal enum AnalogAxis : uint { X, Y }
internal enum JoypadInput : uint
{
    B, Y, Select, Start, Up, Down, Left, Right, A, X, L, R, L2, R2, L3, R3
}
