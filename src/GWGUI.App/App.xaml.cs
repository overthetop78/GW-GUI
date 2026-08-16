using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using GWGUI.App.Localization;
using GWGUI.Infrastructure.Settings;
using GWGUI.Domain.Settings;
using Microsoft.Win32;
using System.Windows.Threading;
using GWGUI.App.Services;
using GWGUI.Infrastructure.HostTools;
using GWGUI.Emulation.Amiga.Cores;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Atari;

namespace GWGUI.App;

public partial class App : Application
{
    private AppTheme _theme;
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args is ["--amiga-core-host", var pipeName, var videoMapName])
        {
            AmigaCoreHost.Run(pipeName, videoMapName);
            Shutdown();
            return;
        }
        if (e.Args is [AtariCoreHostConstants.CommandLineArgument, var atariPipeName, var atariVideoMapName])
        {
            AtariCoreHost.Run(atariPipeName, atariVideoMapName);
            Shutdown();
            return;
        }
        if (e.Args is [AtariCoreOptionProbeConstants.CommandLineArgument, var atariCorePath, var atariCoreKind]
            && Enum.TryParse<AtariCoreKind>(atariCoreKind, out var parsedAtariCoreKind))
        {
            try
            {
                Console.Out.WriteLine(AtariCoreOptionProbe.Inspect(atariCorePath, parsedAtariCoreKind).Count);
                Environment.ExitCode = AtariCoreOptionProbeConstants.SuccessExitCode;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(AtariCoreOptionProbe.DescribeFailure(error));
                Environment.ExitCode = AtariCoreOptionProbeConstants.FailureExitCode;
            }
            Shutdown(Environment.ExitCode);
            return;
        }
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        var directory = StoragePaths.DataDirectory;
        Directory.CreateDirectory(StoragePaths.LogsDirectory);
        var settingsStore = new JsonSettingsStore(Path.Combine(directory, "settings.json"));
        var settings = Task.Run(() => settingsStore.LoadAsync()).GetAwaiter().GetResult();
        var previousGwPath = settings.GwExecutablePath;
        var previousFallbackPath = settings.PreviousGwExecutablePath;
        var previousInstalledVersion = settings.InstalledHostToolsVersion;
        settings.GwExecutablePath = StoragePaths.NormalizeHostToolsPath(settings.GwExecutablePath);
        settings.PreviousGwExecutablePath = StoragePaths.NormalizeHostToolsPath(settings.PreviousGwExecutablePath);
        using (var httpClient = new HttpClient())
        {
            var installations = new GwInstallationManager(httpClient, StoragePaths.HostToolsDirectory)
                .Detect(settings.GwExecutablePath);
            var configuredInstallation = installations.FirstOrDefault(installation =>
                !string.IsNullOrWhiteSpace(settings.GwExecutablePath)
                && string.Equals(installation.ExecutablePath, Path.GetFullPath(settings.GwExecutablePath), StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(configuredInstallation?.Version))
                settings.InstalledHostToolsVersion = configuredInstallation.Version;
            if (!string.IsNullOrWhiteSpace(settings.InstalledHostToolsVersion))
            {
                var managedInstallation = installations.FirstOrDefault(installation =>
                    installation.Managed
                    && string.Equals(installation.Version, settings.InstalledHostToolsVersion, StringComparison.OrdinalIgnoreCase));
                if (managedInstallation is not null) settings.GwExecutablePath = managedInstallation.ExecutablePath;
            }
        }
        var language = UiLanguageResolver.Resolve(settings.Language, CultureInfo.CurrentUICulture);
        if (!string.Equals(settings.Language, language, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(previousGwPath, settings.GwExecutablePath, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(previousFallbackPath, settings.PreviousGwExecutablePath, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(previousInstalledVersion, settings.InstalledHostToolsVersion, StringComparison.OrdinalIgnoreCase))
        {
            settings.Language = language;
            Task.Run(() => settingsStore.SaveAsync(settings)).GetAwaiter().GetResult();
        }
        var culture = UiLanguageResolver.GetCulture(language);
        var uiCulture = UiLanguageResolver.GetUiCulture(language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
        base.OnStartup(e);
        _theme = settings.Theme;
        ThemeManager.Apply(settings.Theme);
        SystemEvents.UserPreferenceChanged += SystemPreferenceChanged;
        MainWindow = new MainWindow(null, initialSettings: settings);
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
        var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
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
        var culture = UiLanguageResolver.GetCulture(language);
        var uiCulture = UiLanguageResolver.GetUiCulture(language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
        LocalizationSource.Instance.Refresh();
        foreach (var window in Windows)
        {
            if (window is MainWindow main) main.RefreshLocalizedContent();
            else if (window is OptionsWindow options) options.RefreshLocalizedContent();
        }
    }
}
