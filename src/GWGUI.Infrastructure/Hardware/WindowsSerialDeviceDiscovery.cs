using System.Management;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using GWGUI.Domain.Hardware;

namespace GWGUI.Infrastructure.Hardware;

[SupportedOSPlatform("windows")]
public sealed partial class WindowsSerialDeviceDiscovery : ISerialDeviceDiscovery
{
    public IReadOnlyList<SerialDevice> FindSerialDevices()
    {
        var devices = new List<SerialDevice>();
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, DeviceID, PNPDeviceID FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");
        foreach (ManagementObject item in searcher.Get())
        {
            var name = item["Name"]?.ToString() ?? "Périphérique série";
            var match = ComPortRegex().Match(name);
            if (!match.Success) continue;
            var port = match.Groups[1].Value.ToUpperInvariant();
            var stableId = item["PNPDeviceID"]?.ToString() ?? item["DeviceID"]?.ToString() ?? port;
            devices.Add(new SerialDevice(port, stableId, name));
        }
        return devices.OrderBy(x => NaturalPortNumber(x.Port)).ThenBy(x => x.Port).ToArray();
    }

    private static int NaturalPortNumber(string port) =>
        int.TryParse(port.AsSpan(3), out var number) ? number : int.MaxValue;

    [GeneratedRegex(@"\((COM\d+)\)", RegexOptions.IgnoreCase)]
    private static partial Regex ComPortRegex();
}
