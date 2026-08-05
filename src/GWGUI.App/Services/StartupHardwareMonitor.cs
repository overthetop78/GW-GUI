using System.IO;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Services;

public sealed record StartupHardwareCheckResult(
    bool Performed,
    IReadOnlyList<ControllerSettings> MissingControllers,
    IReadOnlyList<ControllerSettings> NewControllers);

public sealed class StartupHardwareMonitor(IHardwareRegistry registry, ISettingsStore settingsStore)
{
    public async Task<StartupHardwareCheckResult> CheckAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !File.Exists(settings.GwExecutablePath))
        {
            if (settings.Controllers.Count == 0) return new(false, [], []);
            foreach (var controller in settings.Controllers) controller.IsAvailable = false;
            await settingsStore.SaveAsync(settings, cancellationToken);
            return new(true, settings.Controllers.ToArray(), []);
        }

        var scan = await registry.ScanAsync(settings.GwExecutablePath!, settings.Controllers, cancellationToken);
        settings.Controllers = scan.ConfiguredControllers.ToList();
        var remembered = settings.UnconfiguredControllers.Select(CloneUnavailable).ToList();
        var newControllers = new List<ControllerSettings>();
        foreach (var detected in scan.UnconfiguredControllers)
        {
            var known = remembered.FirstOrDefault(item => SameController(item, detected));
            if (known is null) newControllers.Add(detected);
            else CopyDetection(detected, known);
        }
        settings.UnconfiguredControllers = remembered;
        await settingsStore.SaveAsync(settings, cancellationToken);
        return new(true, settings.Controllers.Where(controller => !controller.IsAvailable).ToArray(), newControllers);
    }

    public static bool SameController(ControllerSettings left, ControllerSettings right) =>
        Same(left.UsbId, right.UsbId) || Same(left.UsbSerialNumber, right.UsbSerialNumber) || Same(left.PnpDeviceId, right.PnpDeviceId);

    private static ControllerSettings CloneUnavailable(ControllerSettings source) => new()
    {
        UsbId = source.UsbId, UsbSerialNumber = source.UsbSerialNumber, PnpDeviceId = source.PnpDeviceId,
        LastUsbLocation = source.LastUsbLocation, VendorId = source.VendorId, ProductId = source.ProductId,
        LastPort = source.LastPort, Model = source.Model, IsAvailable = false
    };

    private static void CopyDetection(ControllerSettings source, ControllerSettings target)
    {
        target.UsbSerialNumber = source.UsbSerialNumber; target.PnpDeviceId = source.PnpDeviceId;
        target.LastUsbLocation = source.LastUsbLocation; target.VendorId = source.VendorId; target.ProductId = source.ProductId;
        target.LastPort = source.LastPort; target.Model = source.Model; target.IsAvailable = source.IsAvailable;
    }

    private static bool Same(string? left, string? right) => !string.IsNullOrWhiteSpace(left) &&
        !string.IsNullOrWhiteSpace(right) && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
