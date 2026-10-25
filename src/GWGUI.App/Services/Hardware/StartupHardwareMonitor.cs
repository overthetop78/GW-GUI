using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Settings.Hardware;
using GWGUI.App.Contracts.Services.Hardware;
using System.IO;
using static GWGUI.Infrastructure.Functions.Hardware.GreaseweazleHardwareScanFunctions;

namespace GWGUI.App.Services.Hardware;

public sealed class StartupHardwareMonitor(IHardwareRegistry registry, ISettingsStore settingsStore, Func<string, bool>? fileExists = null)
{
    private readonly Func<string, bool> exists = fileExists ?? File.Exists;
    public async Task<StartupHardwareCheckResult> CheckAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !exists(settings.GwExecutablePath))
        {
            if (settings.Controllers.Count == 0) return new(false, [], []);
            foreach (var controller in settings.Controllers) controller.IsAvailable = false;
            await settingsStore.SaveAsync(settings, cancellationToken);
            return new(true, settings.Controllers.ToArray(), []);
        }

        var scan = await registry.ScanAsync(settings.GwExecutablePath!, settings.Controllers, cancellationToken);
        settings.Controllers = scan.ConfiguredControllers.ToList();
        var remembered = settings.UnconfiguredControllers.Select(GWGUI.Infrastructure.Functions.Hardware.GreaseweazleHardwareScanFunctions.CloneUnavailable).ToList();
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

    public static bool SameController(ControllerSettings left, ControllerSettings right) => EqualsId(left.UsbId, right.UsbId) || EqualsId(left.UsbSerialNumber, right.UsbSerialNumber) || EqualsId(left.PnpDeviceId, right.PnpDeviceId);

    private static void CopyDetection(ControllerSettings source, ControllerSettings target)
    {
        target.UsbSerialNumber = source.UsbSerialNumber; target.PnpDeviceId = source.PnpDeviceId;
        target.LastUsbLocation = source.LastUsbLocation; target.VendorId = source.VendorId; target.ProductId = source.ProductId;
        target.LastPort = source.LastPort; target.Model = source.Model; target.IsAvailable = source.IsAvailable;
    }

}
