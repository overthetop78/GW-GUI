using GWGUI.Infrastructure.Commands.Execution;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Settings.Hardware;

namespace GWGUI.Infrastructure.Functions.Hardware;

public static class GreaseweazleHardwareScanFunctions
{
    public static bool EqualsId(string? left, string? right) => !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right) && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    public static ControllerSettings CloneUnavailable(ControllerSettings source) => new()
    {
        UsbId = source.UsbId,
        UsbSerialNumber = source.UsbSerialNumber,
        PnpDeviceId = source.PnpDeviceId,
        LastUsbLocation = source.LastUsbLocation,
        VendorId = source.VendorId,
        ProductId = source.ProductId,
        LastPort = source.LastPort,
        Model = source.Model,
        IsAvailable = false
    };

    internal static bool CanUseInfo(GwExecutionResult result, GwDeviceInfo information, SerialDevice serialDevice)
    {
        if (result.IsSuccess) return true;
        if (!information.HasNetworkWarning ||
            string.IsNullOrWhiteSpace(information.Model) ||
            string.IsNullOrWhiteSpace(information.Port) ||
            !string.Equals(information.Port, serialDevice.Port, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(information.FirmwareVersion) &&
            string.IsNullOrWhiteSpace(information.SerialNumber)) return false;

        return string.IsNullOrWhiteSpace(information.SerialNumber) || string.IsNullOrWhiteSpace(serialDevice.UsbSerialNumber) || string.Equals(information.SerialNumber, serialDevice.UsbSerialNumber, StringComparison.OrdinalIgnoreCase);
    }
}
