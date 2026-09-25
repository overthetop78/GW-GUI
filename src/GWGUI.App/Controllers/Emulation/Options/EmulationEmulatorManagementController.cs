using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.Emulation;

namespace GWGUI.App.Controllers.Emulation.Options;

internal sealed class EmulationEmulatorManagementController
{
    private readonly IEmulationEmulatorManager _manager;
    private readonly Func<IEmulationConfiguration> _getConfiguration;
    private readonly Action<IEmulationConfiguration> _setConfiguration;
    private readonly Func<bool> _hasSavedConfiguration;
    private EmulationCoreManagementPanel _view = null!;
    private CancellationTokenSource? _operation;
    private bool _loading;
    private bool _busy;
    private int _emulatorCount;

    internal EmulationEmulatorManagementController(IEmulationEmulatorManager manager,
        Func<IEmulationConfiguration> getConfiguration,
        Action<IEmulationConfiguration> setConfiguration,
        Func<bool> hasSavedConfiguration)
    {
        _manager = manager;
        _getConfiguration = getConfiguration;
        _setConfiguration = setConfiguration;
        _hasSavedConfiguration = hasSavedConfiguration;
    }

    internal event EventHandler? ConfigurationChanged;

    internal UIElement CreateView()
    {
        _view = new EmulationCoreManagementPanel((key, arguments) => LocExtension.Get(key, arguments));
        _view.Install.Click += InstallClicked;
        _view.Cancel.Click += CancelClicked;
        _view.Emulators.SelectionChanged += EmulatorChanged;
        return _view;
    }

    internal async Task RefreshAsync()
    {
        var configuration = _getConfiguration();
        var installations = await _manager.GetEmulatorInstallationsAsync(configuration);
        if (installations.Count == 0 || installations.Any(item => string.IsNullOrWhiteSpace(item.EmulatorId)))
            throw new InvalidOperationException(nameof(installations));
        if (installations.Select(item => item.EmulatorId).Distinct(StringComparer.Ordinal).Count()
            != installations.Count)
            throw new InvalidOperationException(nameof(installations));
        var selected = await _manager.GetEmulatorInstallationAsync(configuration);
        if (!installations.Any(item => string.Equals(item.EmulatorId, selected.EmulatorId,
                StringComparison.Ordinal)))
            throw new InvalidOperationException(nameof(selected));

        _emulatorCount = installations.Count;
        _loading = true;
        try
        {
            _view.Emulators.ItemsSource = installations.Select(item => item.EmulatorId).ToArray();
            _view.Emulators.SelectedItem = selected.EmulatorId;
            _view.ShowInstallation(selected.InstalledVersion is not null);
            _view.SetStatus(string.Empty);
        }
        finally
        {
            _loading = false;
            SetBusy(_busy);
        }
    }

    private async void EmulatorChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_loading || _view.Emulators.SelectedItem is not string emulatorId) return;
        await RunAsync(async cancellationToken =>
        {
            var configuration = await _manager.UseEmulatorAsync(
                _getConfiguration(), emulatorId, cancellationToken);
            _setConfiguration(configuration);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            await RefreshAsync();
        });
    }

    private async void InstallClicked(object sender, RoutedEventArgs args)
    {
        await RunAsync(async cancellationToken =>
        {
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.SearchingResource));
            var configuration = _getConfiguration();
            var releases = await _manager.FindEmulatorReleasesAsync(configuration, cancellationToken);
            var release = releases.FirstOrDefault(candidate => candidate.IsRequired)
                ?? releases.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    LocExtension.Get(EmulationCoreManagementConstants.NoneFoundResource));
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.DownloadingResource,
                release.DisplayName));
            var progress = new Progress<double>(value => _view.Progress.Value = value);
            var path = await _manager.InstallEmulatorAsync(
                configuration, release, progress, cancellationToken);
            await RefreshAsync();
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.InstalledPathResource, path));
        });
    }

    private void CancelClicked(object sender, RoutedEventArgs args) => _operation?.Cancel();

    private async Task RunAsync(Func<CancellationToken, Task> action)
    {
        _operation?.Dispose();
        _operation = new CancellationTokenSource();
        SetBusy(true);
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
        finally { SetBusy(false); }
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        _view.Emulators.IsEnabled = !busy && !_hasSavedConfiguration() && _emulatorCount > 1;
        _view.Install.IsEnabled = !busy;
        _view.Cancel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        _view.Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (!busy) _view.Progress.Value = EmulationCoreManagementConstants.InitialProgress;
    }
}
