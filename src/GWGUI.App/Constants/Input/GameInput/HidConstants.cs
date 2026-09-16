namespace GWGUI.App.Constants.Input.GameInput;

internal static class HidConstants
{
    internal const string HidLibrary = "hid.dll";
    internal const string KernelLibrary = "kernel32.dll";
    internal const int StatusSuccess = 0x00110000;
    internal const int MaximumCapabilityCount = 1024;
    internal const int MaximumUsageRange = 1024;
    internal const uint MaximumUsageListLength = 4096;
    internal const ushort GenericDesktopUsagePage = 0x01;
    internal const ushort HatSwitchUsage = 0x39;
    internal const int HatSwitchPositionCount = 8;
    internal const ushort MaximumSignedValueBitSize = 32;
    internal const uint FileShareRead = 0x00000001;
    internal const uint FileShareWrite = 0x00000002;
    internal const uint OpenExisting = 3;
    internal const uint NoDesiredAccess = 0;
    internal const uint NoFileAttributes = 0;
    internal const byte UnnumberedReportId = 0;
    internal const float InactiveControlValue = 0f;
    internal const float ActiveControlValue = 1f;
    internal const float NeutralAxisValue = 0.5f;
    internal const double MinimumNormalizedValue = 0d;
    internal const double MaximumNormalizedValue = 1d;
}
