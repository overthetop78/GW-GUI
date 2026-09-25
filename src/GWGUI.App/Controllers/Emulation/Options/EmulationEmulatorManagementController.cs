using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.Emulation;

namespace GWGUI.App.Controllers.Emulation.Options;

internal sealed class EmulationEmulatorManagementController : IAsyncDisposable
{
    private readonly IEmulationEmulatorManager _manager;
    private readonly Func<IEmulationConfiguration> _getConfiguration;
    private readonly Action<IEmulationConfiguration> _setConfiguration;
    private readonly Func<bool> _hasSavedConfiguration;
    private EmulationCoreManagementPanel? _view;
    private CancellationTokenSource? _operation;
    private Task _operationTask = Task.CompletedTask;
    private bool _loading;
    private bool _busy;
    private bool _disposed;
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        DetachView();
        _view = new EmulationCoreManagementPanel((key, arguments) => LocExtension.Get(key, arguments));
        _view.Install.Click += InstallClicked;
        _view.Cancel.Click += CancelClicked;
        _view.Emulators.SelectionChanged += EmulatorChanged;
        return _view;
    }

    private void DetachView()
    {
        if (_view is null) return;
        _view.Install.Click -= InstallClicked;
        _view.Cancel.Click -= CancelClicked;
        _view.Emulators.SelectionChanged -= EmulatorChanged;
        _view = null;
    }

    internal async Task RefreshAsync()
    {
        if (_disposed || _view is not { } view) return;
        var configuration = _getConfiguration();
        var installations = await _manager.GetEmulatorInstallationsAsync(configuration);
        if (_disposed || !ReferenceEquals(_view, view)) return;
        if (installations.Count == 0 || installations.Any(item => string.IsNullOrWhiteSpace(item.EmulatorId)))
            throw new InvalidOperationException(nameof(installations));
        if (installations.Select(item => item.EmulatorId).Distinct(StringComparer.Ordinal).Count()
            != installations.Count)
            throw new InvalidOperationException(nameof(installations));
        var selected = await _manager.GetEmulatorInstallationAsync(configuration);
        if (_disposed || !ReferenceEquals(_view, view)) return;
        if (!installations.Any(item => string.Equals(item.EmulatorId, selected.EmulatorId,
                StringComparison.Ordinal)))
            throw new InvalidOperationException(nameof(selected));

        _emulatorCount = installations.Count;
        _loading = true;
        try
        {
            view.Emulators.ItemsSource = installations.Select(item => item.EmulatorId).ToArray();
            view.Emulators.SelectedItem = selected.EmulatorId;
            view.ShowInstallation(selected.InstalledVersion is not null);
            view.SetStatus(string.Empty);
        }
        finally
        {
            _loading = false;
            SetBusy(_busy);
        }
    }

    private async void EmulatorChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_disposed || _loading || sender is not ComboBox { SelectedItem: string emulatorId }) return;
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
            var view = _view ?? throw new ObjectDisposedException(nameof(EmulationEmulatorManagementController));
            view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.SearchingResource));
            var configuration = _getConfiguration();
            var releases = await _manager.FindEmulatorReleasesAsync(configuration, cancellationToken);
            var release = releases.FirstOrDefault(candidate => candidate.IsRequired)
                ?? releases.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    LocExtension.Get(EmulationCoreManagementConstants.NoneFoundResource));
            view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.DownloadingResource,
                release.DisplayName));
            var progress = new Progress<double>(value =>
            {
                if (!_disposed && ReferenceEquals(_view, view)) view.Progress.Value = value;
            });
            var path = await _manager.InstallEmulatorAsync(
                configuration, release, progress, cancellationToken);
            await RefreshAsync();
            if (!_disposed && ReferenceEquals(_view, view))
                view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.InstalledPathResource, path));
        });
    }

    private void CancelClicked(object sender, RoutedEventArgs args) => _operation?.Cancel();

    private Task RunAsync(Func<CancellationToken, Task> action)
    {
        if (_disposed) return Task.CompletedTask;
        var operation = new CancellationTokenSource();
        _operation = operation;
        _operationTask = RunOperationAsync(action, operation);
        return _operationTask;
    }

    private async Task RunOperationAsync(Func<CancellationToken, Task> action,
        CancellationTokenSource operation)
    {
        SetBusy(true);
        try { await action(operation.Token); }
        catch (OperationCanceledException)
        {
            if (!_disposed && _view is { } view)
                view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.CancelledResource));
        }
        catch (Exception error)
        {
            if (!_disposed && _view is { } view)
                ControlErrorPresenter.ShowUnexpected(view, error, ControlErrorContexts.EmulatorManagement,
                    LocExtension.Get(EmulationCoreManagementConstants.EmulatorResource));
        }
        finally
        {
            if (ReferenceEquals(_operation, operation))
            {
                _operation = null;
                SetBusy(false);
            }
            operation.Dispose();
        }
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        if (_disposed || _view is not { } view) return;
        view.Emulators.IsEnabled = !busy && !_hasSavedConfiguration() && _emulatorCount > 1;
        view.Install.IsEnabled = !busy;
        view.Cancel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        view.Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (!busy) view.Progress.Value = EmulationCoreManagementConstants.InitialProgress;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        DetachView();
        var operation = _operation;
        try
        {
            operation?.Cancel();
            await _operationTask;
        }
        finally
        {
            if (ReferenceEquals(_operation, operation)) _operation = null;
            operation?.Dispose();
            _operationTask = Task.CompletedTask;
            ConfigurationChanged = null;
        }
    }
}
