using System.Globalization;
using System.IO;
using System.Windows;
using GWGUI.App.Localization;
using GWGUI.Infrastructure.Settings;
using GWGUI.Domain.Settings;
using Microsoft.Win32;
using System.Windows.Threading;
using GWGUI.App.Services;

namespace GWGUI.App;

public partial class App : Application
{
    private AppTheme _theme;
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
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
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        base.OnExit(e);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        foreach (Window window in Windows) ThemeManager.ApplyWindowTheme(window);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var path = ErrorLog.Write(e.Exception, "WPF dispatcher");
        var detail = path is null ? e.Exception.Message : LocExtension.Get("Error.LogSaved", path);
        MessageBox.Show(LocExtension.Get("Error.Unexpected", detail), LocExtension.Get("Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception) ErrorLog.Write(exception, "AppDomain unhandled exception");
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ErrorLog.Write(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }

    public void SetTheme(AppTheme theme) { _theme = theme; ThemeManager.Apply(theme); }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Window window) return;
        ThemeManager.ApplyWindowTheme(window);
        window.Dispatcher.BeginInvoke(() => ThemeManager.ApplyWindowTheme(window), DispatcherPriority.ApplicationIdle);
    }

    public void SetLanguage(string language)
    {
        var culture = CultureInfo.GetCultureInfo(language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        LocalizationSource.Instance.Refresh();
        foreach (var window in Windows)
        {
            if (window is MainWindow main) main.RefreshLocalizedContent();
            else if (window is OptionsWindow options) options.RefreshLocalizedContent();
        }
    }
}
