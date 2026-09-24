namespace GWGUI.App.Constants.Input.GameInput;

internal static class GameInputConstants
{
    internal const string DeviceIdPrefix = "gameinput:";
    internal const uint MaximumRawReportByteCount = 65_536;
    internal const int MaximumDeviceCallbackTraceLineCount = 256;
    internal const int MaximumDisplayNameByteCount = 512;
    internal const int MaximumPnpPathByteCount = 2_048;
    internal const uint MaximumControllerLabelCount = 1_024;
    internal static readonly TimeSpan CallbackUnregisterRetryInterval = TimeSpan.FromMilliseconds(10);
}
