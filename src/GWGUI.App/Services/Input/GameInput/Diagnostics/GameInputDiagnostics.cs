using GWGUI.App.Constants.Input.GameInput;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputDiagnostics
{
    private static readonly Dictionary<string, string> EnumerationDiagnostics =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> RawEnumerationDiagnostics =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<string> DeviceCallbackTraceLines = [];

    internal static string LastRead { get; set; } = string.Empty;
    internal static string LastDetailedRead { get; set; } = string.Empty;
    internal static string LastCallback { get; set; } = string.Empty;
    internal static string Enumeration => string.Join(Environment.NewLine,
        EnumerationDiagnostics.OrderBy(item => item.Key).Select(item => item.Value));
    internal static string RawEnumeration => string.Join(Environment.NewLine,
        RawEnumerationDiagnostics.OrderBy(item => item.Key).Select(item => item.Value));
    internal static string DeviceCallbackTrace => string.Join(Environment.NewLine, DeviceCallbackTraceLines);

    internal static string DeviceEnumerationFailure(GameInputKind filter) =>
        LocExtension.Get(GameInputDiagnosticResourceKeys.DeviceEnumerationFailure, filter);

    internal static string SystemButtonRegistrationFailure =>
        LocExtension.Get(GameInputDiagnosticResourceKeys.SystemButtonRegistrationFailure);

    internal static void RecordRefreshRegistrationFailure(GameInputKind filter, int result) =>
        LastCallback = LocExtension.Get(
            GameInputDiagnosticResourceKeys.RefreshRegistrationFailure, filter, result);

    internal static void RecordRefreshUnregisterFailure(ulong token) =>
        LastCallback = LocExtension.Get(
            GameInputDiagnosticResourceKeys.RefreshUnregisterFailure, token);

    internal static void RecordDeviceCallbackQueueFailure(Exception exception) =>
        LastCallback = Failure(GameInputDiagnosticResourceKeys.DeviceCallbackQueueFailure, exception);

    internal static void RecordDeviceCallbackFailure(Exception exception) =>
        LastCallback = Failure(GameInputDiagnosticResourceKeys.DeviceCallbackFailure, exception);

    internal static void RecordSystemButtonCallbackFailure(Exception exception) =>
        LastCallback = Failure(GameInputDiagnosticResourceKeys.SystemButtonCallbackFailure, exception);

    internal static void RecordRawReadingCallbackFailure(Exception exception) =>
        LastDetailedRead = Failure(GameInputDiagnosticResourceKeys.RawReadingCallbackFailure, exception);

    internal static void RecordReadFailure(string id, GameInputKind kind, int result) =>
        LastRead = LocExtension.Get(GameInputDiagnosticResourceKeys.ReadFailure, id, kind, result);

    internal static void RecordControllerArraysUnavailable(
        string id, GameInputKind kind, bool hasGamepad) =>
        LastRead = LocExtension.Get(
            GameInputDiagnosticResourceKeys.ControllerArraysUnavailable, id, kind, hasGamepad);

    internal static void RecordControllerReading(
        string id, GameInputKind kind, bool hasGamepad,
        uint axisCount, uint buttonCount, uint switchCount) =>
        LastRead = LocExtension.Get(
            GameInputDiagnosticResourceKeys.ControllerReading,
            id, kind, hasGamepad, axisCount, buttonCount, switchCount);

    internal static void RecordReadInteropFailure(string id, Exception exception) =>
        LastRead = LocExtension.Get(
            GameInputDiagnosticResourceKeys.InteropFailure,
            id, exception.HResult, exception.GetType().Name);

    internal static void RecordDetailedReadFailure(string id, GameInputKind kind, int result) =>
        LastDetailedRead = LocExtension.Get(
            GameInputDiagnosticResourceKeys.ReadFailure, id, kind, result);

    internal static void RecordDetailedReading(string id, GameInputKind kind) =>
        LastDetailedRead = LocExtension.Get(
            GameInputDiagnosticResourceKeys.DetailedReading, id, kind);

    internal static void RecordDetailedReadInteropFailure(string id, Exception exception) =>
        LastDetailedRead = LocExtension.Get(
            GameInputDiagnosticResourceKeys.InteropFailure,
            id, exception.HResult, exception.GetType().Name);

    internal static void RecordMissingRawReport() =>
        LastDetailedRead += GameInputDiagnosticConstants.MissingRawReportSuffix;
    internal static void RecordRawReportSize(nuint size) =>
        LastDetailedRead += string.Format(
            CultureInfo.InvariantCulture, GameInputDiagnosticConstants.RawReportSizeFormat, size);
    internal static void RecordRawReportCopy(nuint size, nuint copied) =>
        LastDetailedRead += string.Format(
            CultureInfo.InvariantCulture, GameInputDiagnosticConstants.RawReportCopyFormat, size, copied);

    internal static void RecordDeviceCallback(
        IntPtr context,
        ulong token,
        GameInputDeviceEntry entry,
        GameInputDeviceStatus currentStatus,
        GameInputDeviceStatus previousStatus)
    {
        DeviceCallbackTraceLines.Add(
            string.Format(CultureInfo.InvariantCulture,
                GameInputDiagnosticConstants.CallbackTraceFormat,
                context.ToInt64(), token, entry.Descriptor.VidPid, entry.Id,
                (uint)currentStatus, (uint)previousStatus));
        if (DeviceCallbackTraceLines.Count > GameInputConstants.MaximumDeviceCallbackTraceLineCount)
            DeviceCallbackTraceLines.RemoveAt(0);
    }

    internal static void RemoveDevice(string id)
    {
        EnumerationDiagnostics.Remove(id);
        RawEnumerationDiagnostics.Remove(id);
    }

    internal static void CaptureRawDevice(
        string id,
        IGameInputDevice device,
        ulong token,
        IntPtr context,
        ulong timestamp,
        GameInputDeviceStatus currentStatus,
        GameInputDeviceStatus previousStatus)
    {
        if (device.GetDeviceInfo(out var infoPointer) < 0 || infoPointer == IntPtr.Zero) return;
        var info = Marshal.PtrToStructure<GameInputDeviceInfo>(infoPointer);
        var deviceInfoSize = Marshal.SizeOf<GameInputDeviceInfo>();
        var controllerInfoSize = Marshal.SizeOf<GameInputControllerInfo>();
        var gamepadInfoSize = Marshal.SizeOf<GameInputGamepadInfo>();
        var chunks = new List<string>
        {
            Format(GameInputDiagnosticConstants.CallbackTokenFormat, token),
            Format(GameInputDiagnosticConstants.ContextFormat, context.ToInt64()),
            Format(GameInputDiagnosticConstants.DeviceIUnknownFormat, GetIUnknownValue(device)),
            Format(GameInputDiagnosticConstants.TimestampFormat, timestamp),
            Format(GameInputDiagnosticConstants.CurrentStatusFormat, (uint)currentStatus),
            Format(GameInputDiagnosticConstants.PreviousStatusFormat, (uint)previousStatus),
            Format(GameInputDiagnosticConstants.DeviceInfoPointerFormat, infoPointer.ToInt64()),
            Format(GameInputDiagnosticConstants.DeviceInfoBytesFormat, deviceInfoSize, ReadHex(infoPointer, deviceInfoSize)),
            Format(GameInputDiagnosticConstants.DisplayNamePointerFormat, info.DisplayName.ToInt64()),
            Format(GameInputDiagnosticConstants.DisplayNameBytesFormat,
                ReadNullTerminatedHex(info.DisplayName, GameInputConstants.MaximumDisplayNameByteCount)),
            Format(GameInputDiagnosticConstants.PnpPathPointerFormat, info.PnpPath.ToInt64()),
            Format(GameInputDiagnosticConstants.PnpPathBytesFormat,
                ReadNullTerminatedHex(info.PnpPath, GameInputConstants.MaximumPnpPathByteCount)),
            Format(GameInputDiagnosticConstants.ControllerInfoPointerFormat, info.ControllerInfo.ToInt64()),
            Format(GameInputDiagnosticConstants.ControllerInfoBytesFormat, controllerInfoSize, ReadHex(info.ControllerInfo, controllerInfoSize)),
            Format(GameInputDiagnosticConstants.GamepadInfoPointerFormat, info.GamepadInfo.ToInt64()),
            Format(GameInputDiagnosticConstants.GamepadInfoBytesFormat, gamepadInfoSize, ReadHex(info.GamepadInfo, gamepadInfoSize))
        };
        if (info.ControllerInfo != IntPtr.Zero)
        {
            var controller = Marshal.PtrToStructure<GameInputControllerInfo>(info.ControllerInfo);
            chunks.Add(Format(
                GameInputDiagnosticConstants.AxisLabelsPointerFormat,
                controller.AxisLabels.ToInt64()));
            chunks.Add(Format(
                GameInputDiagnosticConstants.AxisLabelsBytesFormat,
                controller.AxisCount * sizeof(int),
                ReadHex(controller.AxisLabels, checked((int)controller.AxisCount * sizeof(int)))));
            chunks.Add(Format(
                GameInputDiagnosticConstants.ButtonLabelsPointerFormat,
                controller.ButtonLabels.ToInt64()));
            chunks.Add(Format(
                GameInputDiagnosticConstants.ButtonLabelsBytesFormat,
                controller.ButtonCount * sizeof(int),
                ReadHex(controller.ButtonLabels, checked((int)controller.ButtonCount * sizeof(int)))));
            chunks.Add(Format(
                GameInputDiagnosticConstants.SwitchInfoPointerFormat,
                controller.SwitchInfo.ToInt64()));
        }
        RawEnumerationDiagnostics[id] = string.Join(Environment.NewLine, chunks);
    }

    internal static void RecordEnumeration(
        GameInputDeviceDescriptor descriptor,
        GameInputDeviceInfo info,
        IGameInputMapper? mapper)
    {
        var controller = info.ControllerInfo == IntPtr.Zero
            ? default
            : Marshal.PtrToStructure<GameInputControllerInfo>(info.ControllerInfo);
        var axisLabels = ReadInt32Array(controller.AxisLabels, controller.AxisCount);
        var buttonLabels = ReadInt32Array(controller.ButtonLabels, controller.ButtonCount);
        var mappings = new List<string>();
        if (mapper is not null)
        {
            foreach (var axis in Enum.GetValues<GameInputGamepadAxes>())
                if (axis != 0 && mapper.GetGamepadAxisMappingInfo(axis, out var mapping))
                    mappings.Add(Format(
                        GameInputDiagnosticConstants.AxisMappingFormat,
                        axis, mapping.ControllerElementKind, mapping.ControllerIndex,
                        mapping.IsInverted, mapping.FromTwoButtons,
                        mapping.ButtonMinIndexValue, mapping.ReferenceDirection));
            foreach (var button in Enum.GetValues<GameInputGamepadButtons>())
                if (button != 0 && mapper.GetGamepadButtonMappingInfo(button, out var mapping))
                    mappings.Add(Format(
                        GameInputDiagnosticConstants.ButtonMappingFormat,
                        button, mapping.ControllerElementKind, mapping.ControllerIndex,
                        mapping.IsInverted, mapping.SwitchPosition));
        }
        EnumerationDiagnostics[descriptor.Id] = string.Join(Environment.NewLine, new[]
        {
            LocExtension.Get(GameInputDiagnosticResourceKeys.ResolvedName, descriptor.ProductName),
            LocExtension.Get(GameInputDiagnosticResourceKeys.GameInputName, descriptor.GameInputDisplayName),
            LocExtension.Get(GameInputDiagnosticResourceKeys.VidPid, info.VendorId, info.ProductId),
            LocExtension.Get(GameInputDiagnosticResourceKeys.DeviceId, info.DeviceId.ToHex()),
            LocExtension.Get(GameInputDiagnosticResourceKeys.DeviceRootId, info.DeviceRootId.ToHex()),
            LocExtension.Get(GameInputDiagnosticResourceKeys.ContainerId, info.ContainerId),
            LocExtension.Get(GameInputDiagnosticResourceKeys.Family, info.DeviceFamily),
            LocExtension.Get(GameInputDiagnosticResourceKeys.Usage, info.Usage.Page, info.Usage.Id),
            LocExtension.Get(GameInputDiagnosticResourceKeys.SupportedInput,
                info.SupportedInput, (int)info.SupportedInput),
            LocExtension.Get(GameInputDiagnosticResourceKeys.ControllerInfo,
                controller.AxisCount, controller.ButtonCount, controller.SwitchCount),
            LocExtension.Get(GameInputDiagnosticResourceKeys.AxisLabels,
                string.Join(GameInputDiagnosticConstants.ListSeparator, axisLabels)),
            LocExtension.Get(GameInputDiagnosticResourceKeys.ButtonLabels,
                string.Join(GameInputDiagnosticConstants.ListSeparator, buttonLabels)),
            LocExtension.Get(GameInputDiagnosticResourceKeys.Mapper,
                mappings.Count == 0
                    ? LocExtension.Get(GameInputDiagnosticResourceKeys.NoMapping)
                    : string.Join(GameInputDiagnosticConstants.ValueSeparator, mappings)),
            LocExtension.Get(GameInputDiagnosticResourceKeys.Pnp, descriptor.PnpPath),
            LocExtension.Get(GameInputDiagnosticResourceKeys.PnpNames,
                string.Join(GameInputDiagnosticConstants.IdentitySeparator, descriptor.WindowsIdentityChain))
        });
    }

    internal static void Clear()
    {
        EnumerationDiagnostics.Clear();
        RawEnumerationDiagnostics.Clear();
        DeviceCallbackTraceLines.Clear();
    }

    private static long GetIUnknownValue(object value)
    {
        var pointer = Marshal.GetIUnknownForObject(value);
        try { return pointer.ToInt64(); }
        finally { Marshal.Release(pointer); }
    }

    private static string ReadHex(IntPtr pointer, int length)
    {
        if (pointer == IntPtr.Zero || length <= 0) return string.Empty;
        var bytes = new byte[length];
        Marshal.Copy(pointer, bytes, 0, length);
        return Convert.ToHexString(bytes);
    }

    private static string ReadNullTerminatedHex(IntPtr pointer, int maximum)
    {
        if (pointer == IntPtr.Zero) return string.Empty;
        var bytes = new List<byte>();
        for (var offset = 0; offset < maximum; offset++)
        {
            var value = Marshal.ReadByte(pointer, offset);
            bytes.Add(value);
            if (value == 0) break;
        }
        return Convert.ToHexString(CollectionsMarshal.AsSpan(bytes));
    }

    private static int[] ReadInt32Array(IntPtr pointer, uint count)
    {
        if (pointer == IntPtr.Zero || count == 0 || count > GameInputConstants.MaximumControllerLabelCount)
            return Array.Empty<int>();
        var values = new int[count];
        Marshal.Copy(pointer, values, 0, checked((int)count));
        return values;
    }

    private static string Failure(string resourceKey, Exception exception) =>
        LocExtension.Get(resourceKey, exception.GetType().Name, exception.HResult);

    private static string Format(string format, params object[] arguments) =>
        string.Format(CultureInfo.InvariantCulture, format, arguments);
}
