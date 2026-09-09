using System.Windows;
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
    private readonly Func<string> _machineId;
    private EmulationCoreManagementPanel _view = null!;
    private CancellationTokenSource? _operation;

    internal EmulationEmulatorManagementController(IEmulationEmulatorManager manager, Func<string> machineId)
    {
        _manager = manager;
        _machineId = machineId;
    }

    internal UIElement CreateView()
    {
        _view = new EmulationCoreManagementPanel((key, arguments) => LocExtension.Get(key, arguments));
        _view.Install.Click += InstallClicked;
        _view.Cancel.Click += CancelClicked;
        return _view;
    }

    internal async Task RefreshAsync()
    {
        var installation = await _manager.GetEmulatorInstallationAsync(_machineId());
        _view.Emulators.ItemsSource = new[] { installation.EmulatorId };
        _view.Emulators.SelectedIndex = 0;
        _view.ShowInstallation(installation.InstalledVersion is not null);
        _view.SetStatus(string.Empty);
    }

    private async void InstallClicked(object sender, RoutedEventArgs args)
    {
        await RunAsync(async cancellationToken =>
        {
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.SearchingResource));
            var releases = await _manager.FindEmulatorReleasesAsync(_machineId(), cancellationToken);
            var release = releases.FirstOrDefault(candidate => candidate.IsRequired)
                ?? releases.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    LocExtension.Get(EmulationCoreManagementConstants.NoneFoundResource));
            _view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.DownloadingResource,
                release.DisplayName));
            var progress = new Progress<double>(value => _view.Progress.Value = value);
            var path = await _manager.InstallEmulatorAsync(_machineId(), release, progress, cancellationToken);
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
        _view.Emulators.IsEnabled = !busy;
        _view.Install.IsEnabled = !busy;
        _view.Cancel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        _view.Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (!busy) _view.Progress.Value = EmulationCoreManagementConstants.InitialProgress;
    }
}
