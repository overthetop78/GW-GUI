using GWGUI.Domain.Settings.Hardware;
namespace GWGUI.Domain.Hardware;

public static class HardwareRoutingPolicy
{
    public static void AssignAutomaticDriveSelections(IList<DriveSettings> drives, string controllerId)
    {
        var onController = drives.Where(drive => string.Equals(drive.ControllerUsbId, controllerId, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (onController.Length > 2) throw new ArgumentException("Automatic IBM PC drive selection supports at most two drives per controller.", nameof(drives));
        for (var index = 0; index < onController.Length; index++) onController[index].Selection = index == 0 ? "A" : "B";
    }

    public static string? DeviceArgument(IReadOnlyCollection<ControllerSettings> controllers, IReadOnlyCollection<DriveSettings> drives, DriveSettings? selectedDrive)
    {
        if (selectedDrive is null) return null;
        var usedControllerIds = drives.Select(drive => drive.ControllerUsbId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var available = controllers.Where(controller => controller.IsAvailable && usedControllerIds.Contains(controller.UsbId)).ToArray();
        if (available.Length <= 1) return null;
        return available.FirstOrDefault(controller =>
            string.Equals(controller.UsbId, selectedDrive.ControllerUsbId, StringComparison.OrdinalIgnoreCase))?.LastPort;
    }

    public static string? DriveArgument(IReadOnlyCollection<DriveSettings> configuredDrives, DriveSettings? selectedDrive)
    {
        if (selectedDrive is null) return null;
        var drivesOnController = configuredDrives.Count(drive =>
            string.Equals(drive.ControllerUsbId, selectedDrive.ControllerUsbId, StringComparison.OrdinalIgnoreCase));
        return drivesOnController > 1 ? selectedDrive.Selection : null;
    }
}
