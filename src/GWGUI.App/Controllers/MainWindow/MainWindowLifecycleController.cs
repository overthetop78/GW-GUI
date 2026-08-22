using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Services.Hardware;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Enums.Services.Navigation;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Navigation;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Hardware;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Operations;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Windows.Shell;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using GWGUI.Infrastructure.Settings;

namespace GWGUI.App.Controllers.MainWindow;

internal sealed class MainWindowLifecycleController(
    Window window,
    Func<AppSettings> settings,
    Action<AppSettings> replaceSettings,
    bool settingsProvidedAtStartup,
    ISettingsStore settingsStore,
    StartupHardwareMonitor startupHardwareMonitor,
    IMessageDialogService dialogs,
    IBusinessDialogService businessDialogs,
    IWindowNavigationService navigation,
    OperationRuntimeController operation,
    DiskImageWorkspaceController diskImageWorkspace,
    MainWindowViewModel viewModel,
    Action<AppSettings> configureEmulation,
    Func<Task> stopEmulation,
    Func<Task> loadCapabilities,
    Action initializeWorkspace,
    Action initializeSelectors,
    Action buildConversionFormats,
    Action loadProfiles,
    Action restoreWindow,
    Action constrainWindow,
    Action refreshReadProfiles,
    Action refreshWriteProfiles,
    Action refreshConvertProfiles,
    Action restoreReadSettings,
    Action restoreWriteSettings,
    Action restoreConversionSettings,
    Action refreshHardware,
    Action<bool> setConsoleVisibility,
    Action updateReadCommand,
    Action updateWriteCommand,
    Action updateConvertCommand,
    Action updateProfileStatus,
    Func<Task> checkHostToolsUpdate,
    Action captureWindow,
    Action captureRead,
    Action captureWrite,
    Action captureProfiles,
    Action captureConversion,
    Action applyTheme)
{
    private bool settingsSaveInProgress;
    private bool closeAfterSettingsSave;

    internal async Task LoadAsync()
    {
        if (!settingsProvidedAtStartup) replaceSettings(await settingsStore.LoadAsync());
        configureEmulation(settings());
        await loadCapabilities();
        initializeWorkspace();
        loadProfiles();
        if (!settingsProvidedAtStartup) { restoreWindow(); constrainWindow(); }
        viewModel.Read.Folder = settings().DefaultImagesFolder;
        initializeSelectors();
        refreshReadProfiles(); refreshWriteProfiles(); refreshConvertProfiles();
        restoreReadSettings(); restoreWriteSettings(); restoreConversionSettings();
        buildConversionFormats();
        refreshHardware();
        await VerifyHardwareAsync();
        setConsoleVisibility(settings().ConsoleExpanded);
        updateReadCommand(); updateProfileStatus();
        _ = checkHostToolsUpdate();
    }

    internal void Closing(System.ComponentModel.CancelEventArgs e)
    {
        diskImageWorkspace.CancelAll();
        if (closeAfterSettingsSave) { diskImageWorkspace.Dispose(); return; }
        e.Cancel = true;
        if (settingsSaveInProgress) return;
        if (operation.IsRunning)
        {
            var answer = dialogs.Show(LocExtension.Get("App.OperationRunningClose"), LocExtension.Get("App.Title"), UserDialogButtons.YesNo, UserDialogIcon.Warning);
            if (answer != UserDialogResult.Yes) return;
            operation.RequestCancellation();
        }
        captureWindow(); captureRead(); captureWrite(); captureProfiles(); captureConversion();
        settingsSaveInProgress = true;
        _ = SaveAndCloseAsync();
    }

    internal async Task ShowPreferencesAsync()
    {
        captureProfiles();
        if (!navigation.ShowOptions(settings())) return;
        captureRead(); captureWrite(); captureConversion(); captureWindow();
        loadProfiles(); refreshReadProfiles(); refreshWriteProfiles(); refreshConvertProfiles();
        viewModel.Read.Folder = settings().DefaultImagesFolder; refreshHardware(); applyTheme();
        await settingsStore.SaveAsync(settings());
        updateReadCommand(); updateWriteCommand(); updateConvertCommand();
    }

    private async Task VerifyHardwareAsync()
    {
        if (settings().Controllers.Count == 0 || string.IsNullOrWhiteSpace(settings().GwExecutablePath) || !File.Exists(settings().GwExecutablePath)) return;
        while (true)
        {
            StartupHardwareCheckResult check;
            try { check = await startupHardwareMonitor.CheckAsync(settings()); }
            catch (Exception exception)
            {
                foreach (var controller in settings().Controllers) controller.IsAvailable = false;
                refreshHardware(); await settingsStore.SaveAsync(settings());
                var path = ErrorLog.Write(exception, "Checking configured hardware at startup");
                var detail = path is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
                dialogs.Show(LocExtension.Get("Hardware.StartupCheckFailed", detail), LocExtension.Get("Hardware.StartupTitle"), icon: UserDialogIcon.Warning);
                check = new(true, settings().Controllers.ToArray(), []);
            }
            if (!check.Performed) return;
            refreshHardware();
            foreach (var controller in check.NewControllers)
            {
                var configure = dialogs.Show(LocExtension.Get("Hardware.NewDetected", controller.Model, controller.LastPort),
                    LocExtension.Get("Hardware.NewDetectedTitle"), UserDialogButtons.YesNo, UserDialogIcon.Question) == UserDialogResult.Yes;
                settings().UnconfiguredControllers.Add(controller);
                await settingsStore.SaveAsync(settings());
                if (configure) navigation.ShowOptions(settings(), OptionsSection.Hardware);
                await settingsStore.SaveAsync(settings()); refreshHardware();
            }
            if (check.MissingControllers.Count == 0) return;
            switch (businessDialogs.ResolveMissingHardware(check.MissingControllers))
            {
                case MissingHardwareChoice.Retry: continue;
                case MissingHardwareChoice.OpenSettings:
                    captureProfiles();
                    if (navigation.ShowOptions(settings()))
                    {
                        loadProfiles(); refreshReadProfiles(); refreshWriteProfiles(); refreshConvertProfiles();
                        viewModel.Read.Folder = settings().DefaultImagesFolder; refreshHardware(); applyTheme();
                        await settingsStore.SaveAsync(settings());
                    }
                    return;
                default: return;
            }
        }
    }

    private async Task SaveAndCloseAsync()
    {
        Exception? failure = null;
        try { await settingsStore.SaveAsync(settings()).ConfigureAwait(false); }
        catch (Exception exception) { failure = exception; }
        var stopping = await window.Dispatcher.InvokeAsync(stopEmulation);
        await stopping.ConfigureAwait(false);
        await operation.WaitForCompletionAsync().ConfigureAwait(false);
        if (window.Dispatcher.HasShutdownStarted || window.Dispatcher.HasShutdownFinished) return;
        try
        {
            await window.Dispatcher.InvokeAsync(() =>
            {
                if (failure is not null)
                {
                    var logPath = ErrorLog.Write(failure, "Saving application settings");
                    var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
                    dialogs.Show(LocExtension.Get("App.SettingsSaveFailed", detail), LocExtension.Get("App.Title"), icon: UserDialogIcon.Warning);
                }
                settingsSaveInProgress = false; closeAfterSettingsSave = true; window.Close();
            }, DispatcherPriority.ApplicationIdle);
        }
        catch (TaskCanceledException) when (window.Dispatcher.HasShutdownStarted || window.Dispatcher.HasShutdownFinished) { }
    }
}
