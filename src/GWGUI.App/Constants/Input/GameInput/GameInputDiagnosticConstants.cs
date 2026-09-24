namespace GWGUI.App.Constants.Input.GameInput;

internal static class GameInputDiagnosticConstants
{
    internal const string MissingRawReportSuffix = "; rawReport=null";
    internal const string RawReportSizeFormat = "; rawSize={0}";
    internal const string RawReportCopyFormat = "; rawSize={0}; copied={1}";
    internal const string CallbackTraceFormat =
        "filter=0x{0:X8} token=0x{1:X16} device={2} id={3} current=0x{4:X8} previous=0x{5:X8}";
    internal const string CallbackTokenFormat = "callbackToken=0x{0:X16}";
    internal const string ContextFormat = "context=0x{0:X16}";
    internal const string DeviceIUnknownFormat = "deviceIUnknown=0x{0:X16}";
    internal const string TimestampFormat = "timestamp=0x{0:X16}";
    internal const string CurrentStatusFormat = "currentStatus=0x{0:X8}";
    internal const string PreviousStatusFormat = "previousStatus=0x{0:X8}";
    internal const string DeviceInfoPointerFormat = "deviceInfoPointer=0x{0:X16}";
    internal const string DeviceInfoBytesFormat = "deviceInfoBytes[{0}]={1}";
    internal const string DisplayNamePointerFormat = "displayNamePointer=0x{0:X16}";
    internal const string DisplayNameBytesFormat = "displayNameBytes={0}";
    internal const string PnpPathPointerFormat = "pnpPathPointer=0x{0:X16}";
    internal const string PnpPathBytesFormat = "pnpPathBytes={0}";
    internal const string ControllerInfoPointerFormat = "controllerInfoPointer=0x{0:X16}";
    internal const string ControllerInfoBytesFormat = "controllerInfoBytes[{0}]={1}";
    internal const string GamepadInfoPointerFormat = "gamepadInfoPointer=0x{0:X16}";
    internal const string GamepadInfoBytesFormat = "gamepadInfoBytes[{0}]={1}";
    internal const string AxisLabelsPointerFormat = "axisLabelsPointer=0x{0:X16}";
    internal const string AxisLabelsBytesFormat = "axisLabelsBytes[{0}]={1}";
    internal const string ButtonLabelsPointerFormat = "buttonLabelsPointer=0x{0:X16}";
    internal const string ButtonLabelsBytesFormat = "buttonLabelsBytes[{0}]={1}";
    internal const string SwitchInfoPointerFormat = "switchInfoPointer=0x{0:X16}";
    internal const string AxisMappingFormat = "{0}->{1}[{2}],inv={3},two={4},min={5},dir={6}";
    internal const string ButtonMappingFormat = "{0}->{1}[{2}],inv={3},pos={4}";
    internal const string ValueSeparator = " | ";
    internal const string IdentitySeparator = " || ";
    internal const string ListSeparator = ", ";
}
