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
    private readonly IEmulationModuleLocalization? _localization;
    private EmulationCoreManagementPanel? _view;
    private CancellationTokenSource? _operation;
    private Task _operationTask = Task.CompletedTask;
    private bool _loading;
    private bool _busy;
    private bool _disposed;
    private int _emulatorCount;
    private EmulationEmulatorInstallation? _selectedInstallation;

    internal EmulationEmulatorManagementController(IEmulationEmulatorManager manager,
        Func<IEmulationConfiguration> getConfiguration,
        Action<IEmulationConfiguration> setConfiguration,
        Func<bool> hasSavedConfiguration,
        IEmulationModuleLocalization? localization = null)
    {
        _manager = manager;
        _getConfiguration = getConfiguration;
        _setConfiguration = setConfiguration;
        _hasSavedConfiguration = hasSavedConfiguration;
        _localization = localization;
    }

    internal event EventHandler? ConfigurationChanged;

    internal UIElement CreateView()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        DetachView();
        _view = new EmulationCoreManagementPanel((key, arguments) => LocExtension.Get(key, arguments));
        _view.Search.Click += SearchClicked;
        _view.Download.Click += InstallClicked;
        _view.Cancel.Click += CancelClicked;
        _view.Emulators.SelectionChanged += EmulatorChanged;
        _view.Versions.SelectionChanged += VersionChanged;
        return _view;
    }

    private void DetachView()
    {
        if (_view is null) return;
        _view.Search.Click -= SearchClicked;
        _view.Download.Click -= InstallClicked;
        _view.Cancel.Click -= CancelClicked;
        _view.Emulators.SelectionChanged -= EmulatorChanged;
        _view.Versions.SelectionChanged -= VersionChanged;
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
            view.Emulators.DisplayMemberPath = nameof(EmulationEmulatorInstallation.DisplayName);
            view.Emulators.SelectedValuePath = nameof(EmulationEmulatorInstallation.EmulatorId);
            view.Emulators.ItemsSource = installations;
            view.Emulators.SelectedValue = selected.EmulatorId;
            view.SetDescription(LocExtension.GetLocalized(_localization,
                selected.DescriptionResourceKey));
            _selectedInstallation = selected;
            view.SetInstalledVersion(selected.InstalledVersion is null
                ? LocExtension.Get(EmulationCoreManagementConstants.NotInstalledResource)
                : LocExtension.Get(EmulationCoreManagementConstants.InstalledResource,
                    selected.InstalledVersion));
            view.ShowReleases(false);
            view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.SearchPromptResource));
        }
        finally
        {
            _loading = false;
            SetBusy(_busy);
        }
    }

    private async void SearchClicked(object sender, RoutedEventArgs args)
    {
        await RunAsync(async cancellationToken =>
        {
            var view = _view ?? throw new ObjectDisposedException(nameof(EmulationEmulatorManagementController));
            view.ShowReleases(false);
            view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.SearchingResource));
            var configuration = _getConfiguration();
            var releases = await _manager.FindEmulatorReleasesAsync(configuration, cancellationToken);
            if (_disposed || !ReferenceEquals(_view, view)) return;
            if (releases.Count == 0)
            {
                view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.NoneFoundResource));
                return;
            }

            var installedVersion = _selectedInstallation?.InstalledVersion;
            var displayed = releases.Select(release => DisplayRelease(release, installedVersion)).ToArray();
            view.Versions.DisplayMemberPath = nameof(EmulationEmulatorRelease.DisplayName);
            view.Versions.SelectedValuePath = nameof(EmulationEmulatorRelease.Id);
            view.Versions.ItemsSource = displayed;
            view.Versions.SelectedItem = displayed.FirstOrDefault(item => item.IsRequired)
                ?? displayed.FirstOrDefault(item => IsInstalled(item, installedVersion))
                ?? displayed[0];
            view.ShowReleases(true);
            view.SetStatus(LocExtension.Get(EmulationCoreManagementConstants.VersionsFoundResource,
                releases.Count));
        });
    }

    private async void EmulatorChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_disposed || _loading || sender is not ComboBox { SelectedValue: string emulatorId }) return;
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
        if (_view?.Versions.SelectedItem is not EmulationEmulatorRelease release) return;
        await RunAsync(async cancellationToken =>
        {
            var view = _view ?? throw new ObjectDisposedException(nameof(EmulationEmulatorManagementController));
            var configuration = _getConfiguration();
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

    private void VersionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (sender is ComboBox { SelectedItem: EmulationEmulatorRelease release })
            ((ComboBox)sender).ToolTip = release.DisplayName;
    }

    private static bool IsInstalled(EmulationEmulatorRelease release, string? installedVersion) =>
        !string.IsNullOrWhiteSpace(installedVersion)
        && (string.Equals(release.Version, installedVersion, StringComparison.Ordinal)
            || string.Equals(release.Id, installedVersion, StringComparison.Ordinal));

    private static EmulationEmulatorRelease DisplayRelease(EmulationEmulatorRelease release,
        string? installedVersion)
    {
        var labels = new List<string>();
        if (release.IsRequired)
            labels.Add(LocExtension.Get(EmulationCoreManagementConstants.RequiredVersionResource));
        if (IsInstalled(release, installedVersion))
            labels.Add(LocExtension.Get(EmulationCoreManagementConstants.ProjectVersionResource));
        return labels.Count == 0 ? release : release with
        {
            DisplayName = $"{release.DisplayName} · {string.Join(" · ", labels)}"
        };
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
        view.Search.IsEnabled = !busy;
        view.Download.IsEnabled = !busy && view.Versions.SelectedItem is not null;
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
