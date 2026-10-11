namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Constants;

internal static class InputConstants
{
    internal const uint NoDevice = 0;
    internal const uint JoypadDevice = 1;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
    internal const uint DeviceMask = 255;
    internal const uint SinclairKeyboard = 259;
    internal const uint CursorJoystick = 257;
    internal const uint KempstonJoystick = 513;
    internal const uint Sinclair1Joystick = 769;
    internal const uint Sinclair2Joystick = 1025;
    internal const uint Timex1Joystick = 1281;
    internal const uint Timex2Joystick = 1537;
    internal const uint FullerJoystick = 1793;
    internal const uint KempstonMouse = 258;
    internal const uint PrimaryPort = 0;
    internal const uint EightyOneKeyboardPort = 1;
    internal const uint FuseKeyboardPort = 2;
    internal const uint LeftAnalogIndex = 0;
    internal const uint MouseXId = 0;
    internal const uint MouseYId = 1;
    internal const uint MouseLeftId = 2;
    internal const uint MouseRightId = 3;
    internal const uint JoypadFireId = 8;
    internal const uint JoypadUpId = 4;
    internal const uint JoypadDownId = 5;
    internal const uint JoypadLeftId = 6;
    internal const uint JoypadRightId = 7;
    internal const uint JoypadMaskId = 256;
    internal const uint MaximumJoypadButtonCount = 16;
    internal const short ActiveState = 1;
    internal const short InactiveState = 0;
    internal const int ConsumedRelativeValue = 0;
}
