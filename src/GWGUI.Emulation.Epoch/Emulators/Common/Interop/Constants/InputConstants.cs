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
    internal const uint PrimaryPort = ExternalCoreApiConstants.PrimaryPort;
    internal const uint MouseXId = ExternalCoreApiConstants.MouseXId;
    internal const uint MouseYId = ExternalCoreApiConstants.MouseYId;
    internal const uint MouseLeftId = ExternalCoreApiConstants.MouseLeftId;
    internal const uint MouseRightId = ExternalCoreApiConstants.MouseRightId;
    internal const uint MouseWheelUpId = ExternalCoreApiConstants.MouseWheelUpId;
    internal const uint MouseWheelDownId = ExternalCoreApiConstants.MouseWheelDownId;
    internal const uint MouseMiddleId = ExternalCoreApiConstants.MouseMiddleId;
    internal const uint JoypadMaskId = ExternalCoreApiConstants.JoypadMaskId;
    internal const uint LeftAnalogIndex = ExternalCoreApiConstants.LeftAnalogIndex;
    internal const uint RightAnalogIndex = ExternalCoreApiConstants.RightAnalogIndex;
    internal const uint AnalogXId = ExternalCoreApiConstants.AnalogXId;
    internal const uint AnalogYId = ExternalCoreApiConstants.AnalogYId;
    internal const uint MaximumJoypadButtonCount = 16;
    internal const uint JoypadSelectId = 2;
    internal const uint JoypadStartId = 3;
    internal const uint JoypadLeftShoulderId = 10;
    internal const uint JoypadRightTriggerId = 13;
    internal const short ActiveState = ExternalCoreApiConstants.ActiveState;
    internal const short InactiveState = ExternalCoreApiConstants.InactiveState;
    internal const int ConsumedRelativeValue = ExternalCoreApiConstants.ConsumedRelativeValue;
}
