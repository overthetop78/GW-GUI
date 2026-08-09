using System.IO;
using System.Windows;
using GWGUI.App.Controls;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Settings;
using Microsoft.Win32;

namespace GWGUI.App.Options;

internal sealed class HostToolsOptionsController
{
    private readonly Window _owner;
    private readonly OptionsHardwareSection _section;
    private readonly HostToolsOptionsState _state;
    private readonly Func<Task> _persistSettings;
    private readonly Action<Exception> _reportError;
    private readonly Func<string, object[], string> _localize;

    public HostToolsOptionsController(
        Window owner,
        OptionsHardwareSection section,
        AppSettings settings,
        IGwInstallationManager manager,
        Func<Task> persistSettings,
        Action<Exception> reportError,
        Func<string, object[], string> localize)
    {
        _owner = owner;
        _section = section;
        _state = new HostToolsOptionsState(settings, manager);
        _persistSettings = persistSettings;
        _reportError = reportError;
        _localize = localize;

        section.GwPath.Text = _state.CurrentPath ?? "";
        RefreshStatus();
        section.BrowseGwRequested += Browse;
        section.DetectHostToolsRequested += Detect;
        section.CheckHostToolsRequested += CheckLatest;
        section.DownloadHostToolsRequested += Download;
        section.RollbackHostToolsRequested += Rollback;
    }

    public string CurrentPath => _section.GwPath.Text;

    public void ApplyTo(AppSettings settings)
    {
        _state.SetCurrentPath(CurrentPath);
        _state.ApplyTo(settings);
    }

    public void RefreshLocalizedContent() => RefreshStatus();

    private async void Browse(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = _localize("Options.ExecutableFilter", []) };
        if (dialog.ShowDialog(_owner) != true) return;
        Select(new HostToolsInstallation(dialog.FileName, null, false));
        await _persistSettings();
    }

    private async void Detect(object sender, RoutedEventArgs e)
    {
        var found = _state.Detect(CurrentPath);
        if (found is null)
        {
            _section.HostToolsState.Text = _localize("HostTools.None", []);
            return;
        }

        Select(found);
        _section.HostToolsState.Text = _localize("HostTools.Detected", [found.ExecutablePath]);
        await _persistSettings();
    }

    private async void CheckLatest(object sender, RoutedEventArgs e) =>
        await WithBusyState(async () =>
        {
            var release = await _state.CheckLatestAsync();
            _section.HostToolsState.Text = _localize("HostTools.Latest", [release.Version]);
            await _persistSettings();
        });

    private async void Download(object sender, RoutedEventArgs e) =>
        await WithBusyState(async () =>
        {
            var release = await _state.CheckLatestAsync();
            if (MessageBox.Show(_owner, _localize("HostTools.DownloadConfirm", [release.Version]),
                    _localize("HostTools.Title", []), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            _section.DownloadProgress.Visibility = Visibility.Visible;
            var progress = new Progress<double>(value => _section.DownloadProgress.Value = value * 100);
            var installed = await _state.InstallAsync(release, progress);
            _section.GwPath.Text = _state.CurrentPath ?? "";
            _section.HostToolsState.Text = _localize("HostTools.Installed", [installed.Version ?? release.Version]);
            await _persistSettings();
        });

    private async void Rollback(object sender, RoutedEventArgs e)
    {
        try { _state.Rollback(CurrentPath); }
        catch (FileNotFoundException)
        {
            MessageBox.Show(_owner, _localize("HostTools.NoPrevious", []), _localize("HostTools.Title", []));
            return;
        }

        _section.GwPath.Text = _state.CurrentPath ?? "";
        RefreshStatus();
        await _persistSettings();
    }

    private void Select(HostToolsInstallation installation)
    {
        _state.SetCurrentPath(CurrentPath);
        _state.Select(installation);
        _section.GwPath.Text = _state.CurrentPath ?? "";
    }

    private async Task WithBusyState(Func<Task> action)
    {
        _section.DownloadAction.IsEnabled = false;
        try { await action(); }
        catch (Exception exception) { _reportError(exception); }
        finally
        {
            _section.DownloadAction.IsEnabled = true;
            _section.DownloadProgress.Visibility = Visibility.Collapsed;
        }
    }

    private void RefreshStatus() => _section.HostToolsState.Text = File.Exists(CurrentPath)
        ? _localize("HostTools.Detected", [CurrentPath])
        : _localize("HostTools.None", []);
}
