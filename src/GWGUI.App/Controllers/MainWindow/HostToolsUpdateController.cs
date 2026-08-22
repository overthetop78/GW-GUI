using GWGUI.Domain.HostTools;
using GWGUI.Domain.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Windows.Shell;

namespace GWGUI.App.Controllers.MainWindow;

internal sealed class HostToolsUpdateController(
    IGwInstallationManager hostTools,
    AppSettings settings,
    MainWindowViewModel viewModel)
{
    internal async Task CheckAsync()
    {
        if (settings.LastHostToolsCheckUtc is DateTimeOffset checkedAt &&
            DateTimeOffset.UtcNow - checkedAt < TimeSpan.FromDays(1))
        {
            Refresh();
            return;
        }

        try
        {
            var release = await hostTools.GetLatestReleaseAsync();
            settings.AvailableHostToolsVersion = release.Version;
            settings.LastHostToolsCheckUtc = DateTimeOffset.UtcNow;
            Refresh();
        }
        catch
        {
            // Update checks are intentionally silent.
        }
    }

    internal void Refresh()
    {
        var available = settings.AvailableHostToolsVersion;
        var installed = settings.InstalledHostToolsVersion;
        var newer = Version.TryParse(available, out var availableVersion) &&
            (!Version.TryParse(installed, out var installedVersion) || availableVersion > installedVersion);
        viewModel.HostToolsUpdateVisibility = newer
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;
        if (newer)
            viewModel.HostToolsUpdateText = LocExtension.Get("HostTools.UpdateAvailable", available!);
    }
}
