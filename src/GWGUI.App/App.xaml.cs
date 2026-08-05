using System.Globalization;
using System.IO;
using System.Windows;
using GWGUI.Infrastructure.Settings;
using GWGUI.Domain.Settings;
using Microsoft.Win32;

namespace GWGUI.App;

public partial class App : Application
{
    private AppTheme _theme;
    protected override void OnStartup(StartupEventArgs e)
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GW GUI");
        var settings = new JsonSettingsStore(Path.Combine(directory, "settings.json")).LoadAsync().GetAwaiter().GetResult();
        var culture = CultureInfo.GetCultureInfo(settings.Language == "en" ? "en" : "fr");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        base.OnStartup(e);
        _theme = settings.Theme;
        ThemeManager.Apply(settings.Theme);
        SystemEvents.UserPreferenceChanged += SystemPreferenceChanged;
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private void SystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (_theme == AppTheme.System) Dispatcher.Invoke(() => ThemeManager.Apply(AppTheme.System));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        SystemEvents.UserPreferenceChanged -= SystemPreferenceChanged;
        base.OnExit(e);
    }

    public void SetTheme(AppTheme theme) { _theme = theme; ThemeManager.Apply(theme); }
}
