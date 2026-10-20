using GWGUI.Emulation.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;


internal static class InputConstants
{
    internal const uint JoypadDevice = 1;
    internal const uint AnalogDevice = 5;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
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
    internal const uint MaximumJoypadButtonCount = 32;
    internal const uint JoypadSelectId = 2;
    internal const uint JoypadStartId = 3;
    internal const uint JoypadLeftShoulderId = 10;
    internal const uint JoypadRightTriggerId = 13;
    internal const short ActiveState = ExternalCoreApiConstants.ActiveState;
    internal const short InactiveState = ExternalCoreApiConstants.InactiveState;
    internal const int ConsumedRelativeValue = ExternalCoreApiConstants.ConsumedRelativeValue;
}
