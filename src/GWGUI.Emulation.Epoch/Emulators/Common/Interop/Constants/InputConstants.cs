using GWGUI.Emulation.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;


internal static class InputConstants
{
    internal const uint JoypadDevice = 1;
    internal const uint AnalogDevice = 5;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
    internal const uint LightGunDevice = 4;
    internal const uint LightGunScreenX = 13;
    internal const uint LightGunScreenY = 14;
    internal const uint LightGunOffscreen = 15;
    internal const uint LightGunTrigger = 2;
    internal const uint LightGunReload = 16;
    // Private carrier bits in the SDK snapshot, outside the native RetroPad mask.
    internal const uint GunEnabledMask = 1u << 31;
    internal const uint GunTriggerMask = 1u << 30;
    internal const uint GunReloadMask = 1u << 29;
    internal const uint PrimaryPort = 0;
    internal const uint MouseXId = 0;
    internal const uint MouseYId = 1;
    internal const uint MouseLeftId = 2;
    internal const uint MouseRightId = 3;
    internal const uint MouseWheelUpId = 4;
    internal const uint MouseWheelDownId = 5;
    internal const uint MouseMiddleId = 6;
    internal const uint JoypadMaskId = 256;
    internal const uint LeftAnalogIndex = 0;
    internal const uint RightAnalogIndex = 1;
    internal const uint AnalogXId = 0;
    internal const uint AnalogYId = 1;
    internal const uint MaximumJoypadButtonCount = 16;
    internal const uint JoypadSelectId = 2;
    internal const uint JoypadStartId = 3;
    internal const uint JoypadLeftShoulderId = 10;
    internal const uint JoypadRightTriggerId = 13;
    internal const short ActiveState = 1;
    internal const short InactiveState = 0;
    internal const int ConsumedRelativeValue = 0;
}
