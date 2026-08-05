using GWGUI.Domain.Settings;

namespace GWGUI.Domain.Hardware;

public static class HardwareRoutingPolicy
{
    public static string? DriveArgument(IReadOnlyCollection<DriveSettings> configuredDrives, DriveSettings? selectedDrive)
    {
        if (selectedDrive is null) return null;
        var drivesOnController = configuredDrives.Count(drive =>
            string.Equals(drive.ControllerUsbId, selectedDrive.ControllerUsbId, StringComparison.OrdinalIgnoreCase));
        return drivesOnController > 1 ? selectedDrive.Selection : null;
    }
}
