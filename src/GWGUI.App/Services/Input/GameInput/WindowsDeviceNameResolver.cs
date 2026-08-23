using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.App.Services.Input.GameInput;

internal static class WindowsDeviceNameResolver
{
    private const uint CrSuccess = 0;
    private static readonly DevPropKey DeviceDescription =
        new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 2);
    private static readonly DevPropKey Manufacturer =
        new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 13);
    private static readonly DevPropKey FriendlyName =
        new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 14);
    private static readonly DevPropKey BusReportedDescription =
        new(new Guid("540B947E-8B40-45BC-A8A2-6A0B894CBDA2"), 4);

    internal static string? ResolveProductName(string devicePath)
    {
        var instanceId = ExtractInstanceId(devicePath);
        if (instanceId is null || CM_Locate_DevNodeW(out var node, instanceId, 0) != CrSuccess)
            return null;

        for (var depth = 0; depth < 8; depth++)
        {
            var instanceIdAtDepth = GetDeviceId(node);
            if (instanceIdAtDepth?.Contains(@"\ROOT_HUB", StringComparison.OrdinalIgnoreCase) == true)
                break;
            foreach (var value in new[]
            {
                GetString(node, BusReportedDescription),
                GetString(node, FriendlyName)
            })
            {
                var candidate = value?.Trim();
                if (!string.IsNullOrWhiteSpace(candidate) && !IsTransportOrGenericName(candidate))
                    return candidate;
            }
            if (CM_Get_Parent(out node, node, 0) != CrSuccess) break;
        }
        return null;
    }

    internal static bool IsTransportOrGenericName(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        string[] rejected =
        {
            "xbox wireless adapter", "xbox acc", "périphérique de jeu xbox",
            "xinput compatible", "contrôleur de jeu ihm", "hid-compliant game controller",
            "périphérique d’entrée usb", "usb input device", "concentrateur usb",
            "usb hub", "root hub", "contrôleur de bus usb", "usb host controller",
            "contrôleur hôte", "host controller", "usb receiver"
        };
        return rejected.Any(normalized.Contains);
    }

    internal static IReadOnlyList<string> GetCandidates(string devicePath)
    {
        var instanceId = ExtractInstanceId(devicePath);
        if (instanceId is null || CM_Locate_DevNodeW(out var node, instanceId, 0) != CrSuccess)
            return Array.Empty<string>();

        var result = new List<string>();
        for (var depth = 0; depth < 8; depth++)
        {
            var id = GetDeviceId(node);
            Add(result, depth, "Bus", GetString(node, BusReportedDescription));
            Add(result, depth, "Friendly", GetString(node, FriendlyName));
            Add(result, depth, "Description", GetString(node, DeviceDescription));
            Add(result, depth, "Manufacturer", GetString(node, Manufacturer));
            if (!string.IsNullOrWhiteSpace(id)) result.Add($"{depth}:Id={id}");
            if (CM_Get_Parent(out node, node, 0) != CrSuccess) break;
        }
        return result;
    }

    private static void Add(List<string> values, int depth, string kind, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) values.Add($"{depth}:{kind}={value}");
    }

    private static string? ExtractInstanceId(string path)
    {
        const string prefix = @"\\?\";
        var end = path.LastIndexOf("#{", StringComparison.Ordinal);
        return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && end > prefix.Length
            ? path[prefix.Length..end].Replace('#', '\\')
            : null;
    }

    private static string? GetString(uint node, DevPropKey key)
    {
        uint propertyType;
        uint size = 0;
        var result = CM_Get_DevNode_PropertyW(node, ref key, out propertyType, null, ref size, 0);
        if (size == 0 || (result != CrSuccess && result != 0x1A)) return null;
        var buffer = new byte[size];
        if (CM_Get_DevNode_PropertyW(node, ref key, out propertyType, buffer, ref size, 0) != CrSuccess)
            return null;
        return Encoding.Unicode.GetString(buffer, 0, checked((int)size)).TrimEnd('\0');
    }

    private static string? GetDeviceId(uint node)
    {
        var buffer = new StringBuilder(1024);
        return CM_Get_Device_IDW(node, buffer, (uint)buffer.Capacity, 0) == CrSuccess
            ? buffer.ToString()
            : null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DevPropKey
    {
        internal Guid FormatId;
        internal uint PropertyId;
        internal DevPropKey(Guid formatId, uint propertyId)
        {
            FormatId = formatId;
            PropertyId = propertyId;
        }
    }

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern uint CM_Locate_DevNodeW(out uint deviceInstance, string deviceId, uint flags);

    [DllImport("cfgmgr32.dll")]
    private static extern uint CM_Get_Parent(out uint parent, uint child, uint flags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern uint CM_Get_DevNode_PropertyW(uint deviceInstance, ref DevPropKey propertyKey,
        out uint propertyType, byte[]? propertyBuffer, ref uint propertyBufferSize, uint flags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern uint CM_Get_Device_IDW(uint deviceInstance, StringBuilder buffer,
        uint bufferLength, uint flags);
}
