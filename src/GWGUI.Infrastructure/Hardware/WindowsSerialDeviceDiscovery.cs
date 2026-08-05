using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using GWGUI.Domain.Hardware;
using Microsoft.Win32;

namespace GWGUI.Infrastructure.Hardware;

[SupportedOSPlatform("windows")]
public sealed partial class WindowsSerialDeviceDiscovery : ISerialDeviceDiscovery
{
    private static readonly Guid PortsClassGuid = new("4D36E978-E325-11CE-BFC1-08002BE10318");

    public IReadOnlyList<SerialDevice> FindSerialDevices()
    {
        if (!OperatingSystem.IsWindows()) return [];
        var devices = new Dictionary<string, SerialDevice>(StringComparer.OrdinalIgnoreCase);
        var portsClassGuid = PortsClassGuid;
        var infoSet = SetupDiGetClassDevs(ref portsClassGuid, null, IntPtr.Zero, DigcfPresent);
        if (infoSet == InvalidHandleValue) throw new Win32Exception(Marshal.GetLastWin32Error());

        try
        {
            for (uint index = 0; ; index++)
            {
                var info = new SpDevinfoData { Size = (uint)Marshal.SizeOf<SpDevinfoData>() };
                if (!SetupDiEnumDeviceInfo(infoSet, index, ref info))
                {
                    if (Marshal.GetLastWin32Error() == ErrorNoMoreItems) break;
                    continue;
                }

                var port = ReadPortName(infoSet, info);
                if (string.IsNullOrWhiteSpace(port) || !ComPortRegex().IsMatch(port)) continue;
                var pnpId = ReadInstanceId(infoSet, info);
                var hardwareIds = ReadMultiStringProperty(infoSet, info, SpdrpHardwareId);
                var (vid, pid) = ParseVidPid(hardwareIds.FirstOrDefault() ?? pnpId);
                var friendlyName = ReadProperty(infoSet, info, SpdrpFriendlyName) ?? port;
                var manufacturer = ReadProperty(infoSet, info, SpdrpMfg);
                var product = ReadProperty(infoSet, info, SpdrpDeviceDesc);
                var location = ReadProperty(infoSet, info, SpdrpLocationInformation);
                var serial = ExtractUsbSerial(pnpId);
                var stableId = serial ?? pnpId ?? port;
                devices[port] = new SerialDevice(port.ToUpperInvariant(), stableId, friendlyName,
                    vid, pid, manufacturer, product, serial, pnpId, location);
            }
        }
        finally { SetupDiDestroyDeviceInfoList(infoSet); }

        return devices.Values.OrderBy(x => NaturalPortNumber(x.Port)).ThenBy(x => x.Port).ToArray();
    }

    private static string? ReadPortName(IntPtr set, SpDevinfoData info)
    {
        var key = SetupDiOpenDevRegKey(set, ref info, DicpGlobal, 0, DiregDev, KeyRead);
        if (key == InvalidHandleValue) return null;
        try
        {
            using var registryKey = RegistryKey.FromHandle(new Microsoft.Win32.SafeHandles.SafeRegistryHandle(key, ownsHandle: false));
            return registryKey.GetValue("PortName") as string;
        }
        finally { RegCloseKey(key); }
    }

    private static string? ReadInstanceId(IntPtr set, SpDevinfoData info)
    {
        var buffer = new StringBuilder(512);
        return SetupDiGetDeviceInstanceId(set, ref info, buffer, buffer.Capacity, out _) ? buffer.ToString() : null;
    }

    private static string? ReadProperty(IntPtr set, SpDevinfoData info, uint property)
    {
        var buffer = new byte[2048];
        if (!SetupDiGetDeviceRegistryProperty(set, ref info, property, out _, buffer, (uint)buffer.Length, out _)) return null;
        return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
    }

    private static string[] ReadMultiStringProperty(IntPtr set, SpDevinfoData info, uint property) =>
        (ReadProperty(set, info, property) ?? "").Split('\0', StringSplitOptions.RemoveEmptyEntries);

    internal static (int? VendorId, int? ProductId) ParseVidPid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, null);
        var match = VidPidRegex().Match(value);
        return match.Success
            ? (Convert.ToInt32(match.Groups[1].Value, 16), Convert.ToInt32(match.Groups[2].Value, 16))
            : (null, null);
    }

    internal static string? ExtractUsbSerial(string? pnpId)
    {
        if (string.IsNullOrWhiteSpace(pnpId) || !pnpId.StartsWith("USB\\", StringComparison.OrdinalIgnoreCase)) return null;
        var tail = pnpId[(pnpId.LastIndexOf('\\') + 1)..];
        return string.IsNullOrWhiteSpace(tail) || tail.Contains('&') ? null : tail;
    }

    private static int NaturalPortNumber(string port) =>
        int.TryParse(port.AsSpan(3), out var number) ? number : int.MaxValue;

    [GeneratedRegex(@"^COM\d+$", RegexOptions.IgnoreCase)] private static partial Regex ComPortRegex();
    [GeneratedRegex(@"VID_([0-9A-F]{4}).*PID_([0-9A-F]{4})", RegexOptions.IgnoreCase)] private static partial Regex VidPidRegex();

    [StructLayout(LayoutKind.Sequential)] private struct SpDevinfoData { public uint Size; public Guid ClassGuid; public uint DevInst; public IntPtr Reserved; }

    private const uint DigcfPresent = 0x2, SpdrpDeviceDesc = 0, SpdrpHardwareId = 1, SpdrpMfg = 11,
        SpdrpFriendlyName = 12, SpdrpLocationInformation = 13,
        DicpGlobal = 1, DiregDev = 1, KeyRead = 0x20019;
    private const int ErrorNoMoreItems = 259;
    private static readonly IntPtr InvalidHandleValue = new(-1);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, string? enumerator, IntPtr hwndParent, uint flags);
    [DllImport("setupapi.dll", SetLastError = true)] private static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex, ref SpDevinfoData deviceInfoData);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SpDevinfoData deviceInfoData, uint property, out uint propertyRegDataType, byte[] propertyBuffer, uint propertyBufferSize, out uint requiredSize);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool SetupDiGetDeviceInstanceId(IntPtr deviceInfoSet, ref SpDevinfoData deviceInfoData, StringBuilder deviceInstanceId, int deviceInstanceIdSize, out int requiredSize);
    [DllImport("setupapi.dll", SetLastError = true)] private static extern IntPtr SetupDiOpenDevRegKey(IntPtr deviceInfoSet, ref SpDevinfoData deviceInfoData, uint scope, uint hwProfile, uint keyType, uint samDesired);
    [DllImport("setupapi.dll", SetLastError = true)] private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
    [DllImport("advapi32.dll", SetLastError = true)] private static extern int RegCloseKey(IntPtr hKey);
}
