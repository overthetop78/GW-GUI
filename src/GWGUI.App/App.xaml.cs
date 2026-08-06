using System.Globalization;
using System.IO;
using System.Windows;
using GWGUI.App.Localization;
using GWGUI.Infrastructure.Settings;
using GWGUI.Domain.Settings;
using Microsoft.Win32;

namespace GWGUI.App;

public partial class App : Application
{
    private AppTheme _theme;
    protected override void OnStartup(StartupEventArgs e)
    {
        var directory = StoragePaths.DataDirectory;
        var settingsStore = new JsonSettingsStore(Path.Combine(directory, "settings.json"));
        var settings = Task.Run(() => settingsStore.LoadAsync()).GetAwaiter().GetResult();
        var language = UiLanguageResolver.Resolve(settings.Language, CultureInfo.CurrentUICulture);
        if (!string.Equals(settings.Language, language, StringComparison.OrdinalIgnoreCase))
        {
            settings.Language = language;
            Task.Run(() => settingsStore.SaveAsync(settings)).GetAwaiter().GetResult();
        }
        var culture = CultureInfo.GetCultureInfo(language);
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
