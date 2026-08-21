using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Constants;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed class EmulationEmulatorManagementController
{
    private readonly IEmulationEmulatorManager _manager;
    private readonly Func<string> _machineId;
    private readonly EmulationCoreManagementPanel _view = new(
        (key, arguments) => LocExtension.Get(key, arguments));
    private CancellationTokenSource? _operation;

    internal EmulationEmulatorManagementController(IEmulationEmulatorManager manager, Func<string> machineId)
    {
        _manager = manager;
        _machineId = machineId;
        _view.Search.Click += SearchClicked;
        _view.Download.Click += InstallClicked;
        _view.Cancel.Click += CancelClicked;
        _view.Versions.SelectionChanged += VersionSelectionChanged;
        _view.ShowPrompt(LocExtension.Get(EmulationCoreManagementConstants.SearchPromptResource));
    }

    internal UIElement View => _view;

    internal async Task RefreshAsync()
    {
        var installation = await _manager.GetEmulatorInstallationAsync(_machineId());
        _view.Installed.Text = installation.InstalledVersion is null
            ? LocExtension.Get(EmulationCoreManagementConstants.NotInstalledResource)
            : LocExtension.Get(EmulationCoreManagementConstants.InstalledResource, installation.InstalledVersion);
    }

    private async void SearchClicked(object sender, RoutedEventArgs args)
    {
        await RunAsync(async cancellationToken =>
        {
            _view.HideResults();
            _view.SetStatus(string.Empty);
            _view.ShowPrompt(LocExtension.Get(EmulationCoreManagementConstants.SearchingResource));
            var releases = await _manager.FindEmulatorReleasesAsync(_machineId(), cancellationToken);
            _view.Versions.ItemsSource = releases;
            _view.Versions.DisplayMemberPath = nameof(EmulationEmulatorRelease.DisplayName);
            var required = releases.FirstOrDefault(release => release.IsRequired) ?? releases.FirstOrDefault();
            _view.Versions.SelectedItem = required;
            _view.FoundCount.Text = LocExtension.Get(EmulationCoreManagementConstants.VersionsFoundResource,
                releases.Count);
            _view.RequiredVersion.Text = required?.DisplayName ?? string.Empty;
            _view.LatestVersion.Text = releases.LastOrDefault()?.DisplayName ?? string.Empty;
            if (releases.Count == 0)
                _view.ShowPrompt(LocExtension.Get(EmulationCoreManagementConstants.NoneFoundResource));
            else
            {
                _view.SetStatus(string.Empty);
                _view.ShowResults();
            }
        });
    }

    private async void InstallClicked(object sender, RoutedEventArgs args)
    {
        if (_view.Versions.SelectedItem is not EmulationEmulatorRelease release) return;
        await RunAsync(async cancellationToken =>
        {
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.DownloadingResource,
                release.DisplayName));
            var progress = new Progress<double>(value => _view.Progress.Value = value);
            var path = await _manager.InstallEmulatorAsync(_machineId(), release, progress, cancellationToken);
            await RefreshAsync();
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.InstalledPathResource, path));
        }, showProgress: true);
    }

    private void CancelClicked(object sender, RoutedEventArgs args) => _operation?.Cancel();

    private void VersionSelectionChanged(object sender, SelectionChangedEventArgs args) =>
        _view.Versions.ToolTip = (_view.Versions.SelectedItem as EmulationEmulatorRelease)?.DisplayName;

    private async Task RunAsync(Func<CancellationToken, Task> action, bool showProgress = false)
    {
        _operation?.Dispose();
        _operation = new CancellationTokenSource();
        SetBusy(true, showProgress);
        try { await action(_operation.Token); }
        catch (OperationCanceledException)
        {
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.CancelledResource));
        }
        catch (Exception error)
        {
            ControlErrorPresenter.ShowUnexpected(_view, error, ControlErrorContexts.EmulatorManagement,
                LocExtension.Get(EmulationCoreManagementConstants.EmulatorResource));
        }
        finally { SetBusy(false, false); }
    }

    private void SetBusy(bool busy, bool showProgress)
    {
        _view.Search.IsEnabled = !busy;
        _view.Download.IsEnabled = !busy;
        _view.Cancel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        _view.Progress.Visibility = busy && showProgress ? Visibility.Visible : Visibility.Collapsed;
        if (!showProgress) _view.Progress.Value = EmulationCoreManagementConstants.InitialProgress;
    }
}
