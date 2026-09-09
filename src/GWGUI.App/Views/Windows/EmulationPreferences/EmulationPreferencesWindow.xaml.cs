using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Storage;
using GWGUI.Domain.Settings;
using GWGUI.Infrastructure.Settings;
using System.ComponentModel;
using System.IO;
using System.Windows;
using GWGUI.App.Views.Windows.EmulationModuleOptions;
using GWGUI.Emulation;

namespace GWGUI.App.Views.Windows.EmulationPreferences;

public partial class EmulationPreferencesWindow : Window
{
    private readonly AppSettings _settings;
    private readonly ISettingsStore _settingsStore;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private readonly Action<Exception> _reportSaveError;
    private bool _initializing = true;
    private bool _closingAfterSave;
    private bool _closeInProgress;
    public EmulationPreferencesWindow(AppSettings settings,
        ISettingsStore? settingsStore = null, string? dataDirectory = null,
        Action<Exception>? reportSaveError = null)
    {
        InitializeComponent();
        _settings = settings;
        var directory = dataDirectory ?? StoragePaths.DataDirectory;
        _settingsStore = settingsStore ?? new JsonSettingsStore(Path.Combine(directory, "settings.json"));
        _reportSaveError = reportSaveError ?? (error => ShowLoggedError(error, "Saving Emulation options"));
        EmulationSection.EditConfigurationRequested += async (module, configuration) =>
        {
            var window = new EmulationModuleOptionsWindow(module, configuration) { Owner = this };
            window.ShowDialog();
            await EmulationSection.ReloadConfigurationsForWindowAsync();
        };
        EmulationSection.Configure(settings, SaveFromEditorAsync);
        _initializing = false;
        UpdateTitle();
    }

    internal void RefreshLocalizedContent()
    {
        EmulationSection.RefreshLocalizedContent();
        UpdateTitle();
    }

    internal async Task PersistSettingsAsync()
    {
        if (_initializing) return;
        await _saveLock.WaitAsync().ConfigureAwait(false);
        try { await _settingsStore.SaveAsync(_settings).ConfigureAwait(false); }
        finally { _saveLock.Release(); }
    }

    internal async Task SaveFromEditorAsync()
    {
        try { await PersistSettingsAsync(); }
        catch (Exception exception) { _reportSaveError(exception); }
    }

    private void UpdateTitle()
    {
        Title = LocExtension.Get("Options.EmulationTitle");
    }

    private void ShowLoggedError(Exception exception, string context)
    {
        ErrorLog.Write(exception, context);
        var detail = ExceptionDescriptionFunctions.Describe(exception);
        MessageBox.Show(this, LocExtension.Get("Error.Unexpected", detail),
            LocExtension.Get("Error.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => BeginClose();

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_initializing || _closingAfterSave) return;
        e.Cancel = true;
        BeginClose();
    }

    private void BeginClose()
    {
        if (_closeInProgress) return;
        _closeInProgress = true;
        _ = SaveAndCloseAsync();
    }

    private async Task SaveAndCloseAsync()
    {
        try { await PersistSettingsAsync().ConfigureAwait(false); }
        catch (Exception exception)
        {
            ErrorLog.Write(exception, "Saving Emulation options while closing");
            var detail = ExceptionDescriptionFunctions.Describe(exception);
            try
            {
                if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (IsLoaded)
                            MessageBox.Show(this, LocExtension.Get("Error.SaveFailed", detail),
                                LocExtension.Get("Error.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }).Task.ConfigureAwait(false);
            }
            catch (Exception dialogException)
            {
                ErrorLog.Write(dialogException, "Displaying Emulation options save error");
            }
        }

        try
        {
            if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                await Dispatcher.InvokeAsync(() =>
                {
                    if (_closingAfterSave) return;
                    _closingAfterSave = true;
                    _closeInProgress = false;
                    Close();
                }).Task.ConfigureAwait(false);
            else
            {
                _closingAfterSave = true;
                _closeInProgress = false;
            }
        }
        catch (Exception closeException)
        {
            _closingAfterSave = true;
            _closeInProgress = false;
            ErrorLog.Write(closeException, "Closing Emulation options after save");
        }
    }
}

