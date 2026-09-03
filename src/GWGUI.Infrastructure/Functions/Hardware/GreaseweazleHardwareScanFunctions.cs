using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Hardware;

namespace GWGUI.Infrastructure.Functions.Hardware;

internal static class GreaseweazleHardwareScanFunctions
{
    internal static bool CanUseInfo(
        GwExecutionResult result,
        GwDeviceInfo information,
        SerialDevice serialDevice)
    {
        if (result.IsSuccess) return true;
        if (!information.HasNetworkWarning ||
            string.IsNullOrWhiteSpace(information.Model) ||
            string.IsNullOrWhiteSpace(information.Port) ||
            !string.Equals(information.Port, serialDevice.Port, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(information.FirmwareVersion) &&
            string.IsNullOrWhiteSpace(information.SerialNumber)) return false;

        return string.IsNullOrWhiteSpace(information.SerialNumber) ||
               string.IsNullOrWhiteSpace(serialDevice.UsbSerialNumber) ||
               string.Equals(information.SerialNumber, serialDevice.UsbSerialNumber,
                   StringComparison.OrdinalIgnoreCase);
    }
}
