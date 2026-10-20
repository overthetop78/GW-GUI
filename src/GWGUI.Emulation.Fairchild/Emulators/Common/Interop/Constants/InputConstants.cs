using GWGUI.Emulation.Constants;
namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Constants;

internal static class InputConstants
{
    internal const uint JoypadDevice = 1;
    internal const uint JoypadMaskId = ExternalCoreApiConstants.JoypadMaskId;
    internal const uint MaximumJoypadButtonCount = 16;
    internal const uint PrimaryIndex = 0;
    internal const short ActiveState = ExternalCoreApiConstants.ActiveState;
    internal const short InactiveState = ExternalCoreApiConstants.InactiveState;
    internal const int ConsumedRelativeValue = ExternalCoreApiConstants.ConsumedRelativeValue;
}
