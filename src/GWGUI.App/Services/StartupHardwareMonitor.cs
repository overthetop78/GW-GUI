using System.IO;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Services;

public sealed record StartupHardwareCheckResult(bool Performed, IReadOnlyList<ControllerSettings> MissingControllers);

public sealed class StartupHardwareMonitor(IHardwareRegistry registry, ISettingsStore settingsStore)
{
    public async Task<StartupHardwareCheckResult> CheckAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (settings.Controllers.Count == 0)
            return new(false, []);

        if (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !File.Exists(settings.GwExecutablePath))
        {
            foreach (var controller in settings.Controllers) controller.IsAvailable = false;
            await settingsStore.SaveAsync(settings, cancellationToken);
            return new(true, settings.Controllers.ToArray());
        }

        settings.Controllers = (await registry.ScanAsync(settings.GwExecutablePath!, settings.Controllers, cancellationToken)).ToList();
        await settingsStore.SaveAsync(settings, cancellationToken);
        return new(true, settings.Controllers.Where(controller => !controller.IsAvailable).ToArray());
    }
}
