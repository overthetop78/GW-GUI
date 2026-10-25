namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;

internal static class InputConstants
{
    internal const uint NoDevice = 0;
    internal const uint JoypadDevice = 1;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
    internal const uint AnalogDevice = 5;
    internal const uint DeviceMask = 255;
    internal const uint PrimaryPort = 0;
    internal const uint LeftAnalogIndex = 0;
    internal const uint RightAnalogIndex = 1;
    internal const uint AnalogXId = 0;
    internal const uint AnalogYId = 1;
    internal const uint MouseXId = 0;
    internal const uint MouseYId = 1;
    internal const uint MouseLeftId = 2;
    internal const uint MouseRightId = 3;
    internal const uint JoypadBId = 0;
    internal const uint JoypadYId = 1;
    internal const uint JoypadSelectId = 2;
    internal const uint JoypadStartId = 3;
    internal const uint JoypadUpId = 4;
    internal const uint JoypadDownId = 5;
    internal const uint JoypadLeftId = 6;
    internal const uint JoypadRightId = 7;
    internal const uint JoypadAId = 8;
    internal const uint JoypadXId = 9;
    internal const uint JoypadMaskId = 256;
    internal const uint MaximumJoypadButtonCount = 16;
    internal const short ActiveState = 1;
    internal const short InactiveState = 0;
    internal const int ConsumedRelativeValue = 0;
}
