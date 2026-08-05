using GWGUI.Domain.Settings;

namespace GWGUI.Domain.Hardware;

public static class HardwareRoutingPolicy
{
    public static string? DriveArgument(IReadOnlyCollection<DriveSettings> configuredDrives, DriveSettings? selectedDrive) =>
        configuredDrives.Count > 1 ? selectedDrive?.Selection : null;
}
