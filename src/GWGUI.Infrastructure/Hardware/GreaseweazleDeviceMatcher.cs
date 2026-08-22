using GWGUI.Domain.Hardware;
namespace GWGUI.Infrastructure.Hardware;

public static class GreaseweazleDeviceMatcher
{
    public static bool IsCandidate(SerialDevice device) => Score(device) > 0;

    public static int Score(SerialDevice device)
    {
        if (device.VendorId == 0x1209 && device.ProductId == 0x4d69) return 20;
        if (Contains(device.Manufacturer, "Keir Fraser") &&
            (Contains(device.Product, "Greaseweazle") || Contains(device.DisplayName, "Greaseweazle"))) return 20;
        if (Contains(device.Product, "gw-compat") || Contains(device.DisplayName, "gw-compat")) return 19;
        if (device.VendorId == 0x1209 && device.ProductId == 0x0001) return 10;
        if (device.UsbSerialNumber?.StartsWith("GW", StringComparison.OrdinalIgnoreCase) == true) return 10;
        return 0;
    }

    private static bool Contains(string? value, string expected) =>
        value?.Contains(expected, StringComparison.OrdinalIgnoreCase) == true;
}
