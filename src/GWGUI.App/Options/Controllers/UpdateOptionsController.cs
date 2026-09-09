using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.App.Services.Updates;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Updates.Contracts;
using GWGUI.App.Contracts.Updates;

namespace GWGUI.App.Options.Controllers;

internal sealed class UpdateOptionsController : IDisposable
{
    private readonly OptionsUpdatesSection _section;
    private readonly Window _owner;
    private readonly ApplicationUpdateService _applicationService;
    private readonly ModuleUpdateService _moduleService;
    private readonly ModuleDirectoryService _moduleDirectoryService;
    private readonly UpdatePackagePreparationService _packagePreparation;
    private readonly ModuleInstallationService _moduleInstallation;
    private readonly PendingModuleInstallationStore _pendingInstallations;
    private readonly Func<string, object[], string> _localize;
    private readonly Action<Exception> _reportError;
    private readonly Dictionary<string, string> _applicationSelections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _moduleSelections = new(StringComparer.OrdinalIgnoreCase);
    private UpdateSearchResult? _applicationResult;
    private UpdateSearchResult? _moduleResult;
    private CancellationTokenSource? _preparationCancellation;
    private UpdateVisualState _applicationState = UpdateVisualState.Neutral;
    private UpdateVisualState _directoryState = UpdateVisualState.Neutral;
    private UpdateVisualState _modulesState = UpdateVisualState.Current;
    private UpdateVisualState _advancedState = UpdateVisualState.Neutral;
    private ModuleOperationPage _moduleOperationPage = ModuleOperationPage.Module;

    internal ObservableCollection<UpdateComponentRow> ApplicationResults { get; } = [];
    internal ObservableCollection<UpdateComponentRow> ModuleResults { get; } = [];
    internal ObservableCollection<AvailableModuleRow> AvailableModuleResults { get; } = [];

    internal UpdateOptionsController(
        Window owner,
        OptionsUpdatesSection section,
        ApplicationUpdateService applicationService,
        Func<string, object[], string> localize,
        Action<Exception> reportError,
        ModuleUpdateService? moduleService = null,
        ModuleDirectoryService? moduleDirectoryService = null,
        UpdatePackagePreparationService? packagePreparation = null,
        ModuleInstallationService? moduleInstallation = null,
        PendingModuleInstallationStore? pendingInstallations = null)
    {
        _owner = owner;
        _section = section;
        _applicationService = applicationService;
        _moduleService = moduleService ?? new ModuleUpdateService();
        _moduleDirectoryService = moduleDirectoryService ?? new ModuleDirectoryService();
        _packagePreparation = packagePreparation ?? new UpdatePackagePreparationService();
        _pendingInstallations = pendingInstallations ?? new PendingModuleInstallationStore();
        _moduleInstallation = moduleInstallation ?? new ModuleInstallationService(
            packagePreparation: _packagePreparation, isPending: _pendingInstallations.Contains);
        _localize = localize;
        _reportError = reportError;
        _section.Results.ItemsSource = ApplicationResults;
        _section.ModuleResults.ItemsSource = ModuleResults;
        _section.AvailableModuleResults.ItemsSource = AvailableModuleResults;
        _section.SearchRequested += SearchApplication;
        _section.SearchModulesRequested += SearchModules;
        _section.SearchAvailableModulesRequested += SearchAvailableModules;
        _section.InstallAvailableModuleRequested += InstallAvailableModule;
        _section.VersionSelectionChanged += VersionChanged;
        _section.InstallRequested += InstallApplication;
        _section.InstallModulesRequested += InstallModules;
        _section.CancelRequested += Cancel;
        _section.CancelModulesRequested += Cancel;
        _section.InstallModuleFileRequested += InstallModuleFromFile;
        _section.InstallModuleUrlRequested += InstallModuleFromUrl;
        _pendingInstallations.Changed += PendingInstallationsChanged;
        RefreshLocalizedContent();
        PendingInstallationsChanged(this, EventArgs.Empty);
    }

    public void Dispose() => _pendingInstallations.Changed -= PendingInstallationsChanged;

    internal void RefreshLocalizedContent()
    {
        RebuildNavigation();
        if (_applicationResult is null) _section.Status.Text = _localize("Updates.Ready", []);
        else RenderApplication(_applicationResult);
        if (_moduleResult is null) _section.ModuleStatus.Text = _localize("Updates.Ready", []);
        else RenderModules(_moduleResult);
        if (AvailableModuleResults.Count == 0)
            _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectoryReady", []);
    }

    private void RebuildNavigation()
    {
        var items = new List<UpdateNavigationItem>
        {
            new("application", UpdateNavigationPage.Application,
                _localize("Updates.Application", []),
                _localize("Updates.NavigationApplicationSubtitle", []), "\uE946", _applicationState),
            new("directory", UpdateNavigationPage.Directory,
                _localize("Updates.ModuleDirectoryTitle", []),
                _localize("Updates.NavigationDirectorySubtitle", []), "\uE719", _directoryState)
        };
        foreach (var package in EmulationModuleRegistry.Packages
                     .OrderBy(package => LocExtension.GetForModule(package.Module,
                         package.Module.DisplayResourceKey), StringComparer.CurrentCultureIgnoreCase))
        {
            var component = _moduleResult?.Components.FirstOrDefault(value =>
                string.Equals(value.ComponentId, package.Module.Id, StringComparison.OrdinalIgnoreCase));
            var state = component?.Availability switch
            {
                UpdateAvailability.Available => UpdateVisualState.Available,
                UpdateAvailability.UpToDate => UpdateVisualState.Current,
                UpdateAvailability.ApplicationUpdateRequired or UpdateAvailability.Incompatible => UpdateVisualState.Error,
                _ => _modulesState
            };
            items.Add(new($"module:{package.Module.Id}", UpdateNavigationPage.Module,
                LocExtension.GetForModule(package.Module, package.Module.DisplayResourceKey),
                _localize("Updates.InstalledVersion", [package.Manifest.ModuleVersion]),
                "\uE7FC", state, package.Module.Id));
        }
        items.Add(new("advanced", UpdateNavigationPage.Advanced,
            _localize("Updates.ModuleAdvancedInstallationTitle", []),
            _localize("Updates.NavigationAdvancedSubtitle", []), "\uE713", _advancedState));
        _section.SetNavigationItems(items);
    }

    private async void SearchApplication(object sender, RoutedEventArgs e)
    {
        _section.SearchButton.IsEnabled = false;
        _applicationState = UpdateVisualState.Busy;
        RebuildNavigation();
        _section.Status.Text = _localize("Updates.Searching", []);
        try
        {
            _applicationResult = await _applicationService.SearchAsync(_applicationSelections);
            RenderApplication(_applicationResult);
            _applicationState = StateFor(_applicationResult);
        }
        catch (Exception exception)
        {
            _section.Status.Text = _localize("Updates.SearchFailed", []);
            _applicationState = UpdateVisualState.Error;
            _reportError(exception);
        }
        finally
        {
            RebuildNavigation();
            if (_preparationCancellation is null) _section.SearchButton.IsEnabled = true;
        }
    }

    private async void SearchModules(object sender, RoutedEventArgs e)
    {
        _section.SearchModulesButton.IsEnabled = false;
        _modulesState = UpdateVisualState.Busy;
        RebuildNavigation();
        _section.ModuleStatus.Text = _localize("Updates.Searching", []);
        try
        {
            _moduleResult = await _moduleService.SearchAsync(_moduleSelections);
            RenderModules(_moduleResult);
            _modulesState = StateFor(_moduleResult);
        }
        catch (Exception exception)
        {
            _section.ModuleStatus.Text = _localize("Updates.SearchFailed", []);
            _modulesState = UpdateVisualState.Error;
            _reportError(exception);
        }
        finally
        {
            RebuildNavigation();
            if (_preparationCancellation is null) _section.SearchModulesButton.IsEnabled = true;
        }
    }

    private async void SearchAvailableModules(object sender, RoutedEventArgs e)
    {
        _section.SearchAvailableModulesButton.IsEnabled = false;
        _directoryState = UpdateVisualState.Busy;
        _section.AvailableModulesProgress.Visibility = Visibility.Visible;
        _section.AvailableModulesProgress.IsIndeterminate = true;
        RebuildNavigation();
        _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectorySearching", []);
        try
        {
            var modules = await _moduleDirectoryService.SearchAsync();
            AvailableModuleResults.Clear();
            foreach (var module in modules)
            {
                var installedVersion = module.InstalledVersion;
                var isInstalled = installedVersion is not null;
                var isDownloaded = _pendingInstallations.Contains(module.Id);
                var details = installedVersion is not null
                    ? _localize("Updates.InstalledVersion", [installedVersion])
                    : isDownloaded
                    ? _localize("Updates.ModuleDownloaded", [])
                    : module.AvailableVersion is null
                    ? _localize("Updates.Incompatible", [])
                    : _localize("Updates.ModuleAvailableVersion", [module.AvailableVersion]);
                AvailableModuleResults.Add(new(module.Id, module.DisplayName, module.CatalogUrl,
                    details, isInstalled, isDownloaded, module.CanInstall && !isDownloaded,
                    isDownloaded ? _localize("Updates.ModuleDownloaded", [])
                        : _localize("Updates.ModuleDirectoryInstalledBadge", [])));
            }
            _section.AvailableModuleStatus.Text = AvailableModuleResults.Count == 0
                ? _localize("Updates.ModuleDirectoryEmpty", [])
                : _localize("Updates.ModuleDirectoryResults", [AvailableModuleResults.Count]);
            _directoryState = UpdateVisualState.Current;
        }
        catch (Exception exception)
        {
            _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectoryFailed", []);
            _directoryState = UpdateVisualState.Error;
            _reportError(exception);
        }
        finally
        {
            _section.AvailableModulesProgress.IsIndeterminate = false;
            _section.AvailableModulesProgress.Visibility = Visibility.Collapsed;
            RebuildNavigation();
            if (_preparationCancellation is null) _section.SearchAvailableModulesButton.IsEnabled = true;
        }
    }

    private void VersionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox { DataContext: UpdateComponentRow row }) return;
        var selections = row.Kind == UpdateComponentKind.Application
            ? _applicationSelections : _moduleSelections;
        if (string.IsNullOrWhiteSpace(row.SelectedVersion)) selections.Remove(row.Id);
        else selections[row.Id] = row.SelectedVersion;
    }

    private void RenderApplication(UpdateSearchResult result)
    {
        Render(result, ApplicationResults);
        _section.Status.Text = ResultStatus(result);
        _section.InstallButton.IsEnabled = CanInstall(result) && _preparationCancellation is null;
    }

    private void RenderModules(UpdateSearchResult result)
    {
        Render(result, ModuleResults);
        _section.ModuleStatus.Text = ResultStatus(result);
        _section.InstallModulesButton.IsEnabled = CanInstall(result) && _preparationCancellation is null;
        _section.FilterModuleResults(_section.SelectedModuleId);
    }

    private void Render(UpdateSearchResult result, ObservableCollection<UpdateComponentRow> target)
    {
        target.Clear();
        foreach (var update in result.Components)
        {
            var displayName = update.Kind == UpdateComponentKind.Application
                ? _localize("Updates.Application", []) : update.ComponentId;
            var status = update.Availability switch
            {
                UpdateAvailability.UpToDate => _localize("Updates.UpToDate", []),
                UpdateAvailability.Available => _localize("Updates.Available", []),
                UpdateAvailability.ApplicationUpdateRequired => _localize("Updates.ApplicationRequired", [update.RequiredHostApiVersion ?? ""]),
                _ => _localize("Updates.Incompatible", [])
            };
            target.Add(new(update.ComponentId, update.Kind, displayName,
                _localize("Updates.InstalledVersion", [update.InstalledVersion]), status,
                update.Releases.Select(release => release.Version).ToArray(), update.SelectedVersion,
                update.Availability == UpdateAvailability.Available, VisualStateFor(update.Availability)));
        }
    }

    private static UpdateVisualState StateFor(UpdateSearchResult result) =>
        result.Components.Any(update => update.Availability is UpdateAvailability.Incompatible
            or UpdateAvailability.ApplicationUpdateRequired)
            ? UpdateVisualState.Error
            : result.Components.Any(update => update.Availability == UpdateAvailability.Available)
                ? UpdateVisualState.Available
                : UpdateVisualState.Current;

    private static UpdateVisualState VisualStateFor(UpdateAvailability availability) => availability switch
    {
        UpdateAvailability.UpToDate => UpdateVisualState.Current,
        UpdateAvailability.Available => UpdateVisualState.Available,
        _ => UpdateVisualState.Error
    };

    private string ResultStatus(UpdateSearchResult result) =>
        result.Components.Any(update => update.Availability != UpdateAvailability.UpToDate)
            ? _localize("Updates.Results", []) : _localize("Updates.NoUpdates", []);

    private async void InstallApplication(object sender, RoutedEventArgs e) =>
        await InstallAsync(UpdateSearchScope.Application);

    private async void InstallModules(object sender, RoutedEventArgs e) =>
        await InstallModuleUpdatesAsync();

    private async Task InstallModuleUpdatesAsync()
    {
        _moduleOperationPage = ModuleOperationPage.Module;
        await InstallAsync(UpdateSearchScope.Modules);
    }

    private async void InstallModuleFromFile(object sender, RoutedEventArgs e)
    {
        _moduleOperationPage = ModuleOperationPage.Advanced;
        var dialog = new OpenFileDialog
        {
            Title = _localize("Updates.ModuleInstallFromFile", []),
            Filter = _localize("Updates.ModuleArchiveFilter", [])
        };
        if (dialog.ShowDialog(_owner) != true) return;
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromFileAsync(dialog.FileName,
                Path.GetFileNameWithoutExtension(dialog.FileName), cancellation));
    }

    private async void InstallModuleFromUrl(object sender, RoutedEventArgs e)
    {
        _moduleOperationPage = ModuleOperationPage.Advanced;
        var catalogUrl = _section.ModuleCatalogUrl.Text.Trim();
        if (catalogUrl.Length == 0)
        {
            _section.ManualInstallationStatus.Text = _localize("Updates.ModuleCatalogUrlRequired", []);
            return;
        }
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromCatalogAsync(catalogUrl, catalogUrl,
                new Progress<double>(value => ActiveModuleProgress().Value = value * 100), cancellation));
    }

    private async void InstallAvailableModule(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: AvailableModuleRow module } || !module.CanInstall
            || _pendingInstallations.Contains(module.Id)) return;
        _moduleOperationPage = ModuleOperationPage.Directory;
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromCatalogAsync(module.CatalogUrl, module.DisplayName,
                new Progress<double>(value => ActiveModuleProgress().Value = value * 100), cancellation));
    }

    private async Task PrepareNewModuleAsync(
        Func<CancellationToken, Task<PendingModuleInstallation>> prepare)
    {
        if (_preparationCancellation is not null) return;
        PendingModuleInstallation? prepared = null;
        try
        {
            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(UpdateSearchScope.Modules, true);
            prepared = await prepare(_preparationCancellation.Token);
            if (!_pendingInstallations.TryAdd(prepared))
            {
                DeletePreparedWork(prepared.WorkingDirectory);
                return;
            }
            SetModuleOperationStatus(_localize("Updates.ModuleDownloaded", []));
            SetModuleVisualState(UpdateVisualState.Current);
            RefreshAvailableModuleRows();
        }
        catch (OperationCanceledException)
        {
            SetModuleOperationStatus(_localize("Updates.PreparationCancelled", []));
            SetModuleVisualState(UpdateVisualState.Neutral);
        }
        catch (Exception exception)
        {
            if (prepared is not null && !_pendingInstallations.Contains(prepared.ModuleId))
                DeletePreparedWork(prepared.WorkingDirectory);
            SetModuleOperationStatus(_localize("Updates.ModuleInstallFailed", []));
            SetModuleVisualState(UpdateVisualState.Error);
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(UpdateSearchScope.Modules, false);
        }
    }

    internal async Task FinishModuleInstallationsAsync(bool confirm = true)
    {
        var installations = _pendingInstallations.Items;
        if (installations.Count == 0 || _preparationCancellation is not null) return;
        if (confirm && MessageBox.Show(_owner, _localize("Updates.FinishModuleInstallationsConfirm", []),
                _localize("Updates.Title", []), MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(UpdateSearchScope.Modules, true);
            var launch = await _packagePreparation.CreateModuleLaunchAsync(
                installations, _preparationCancellation.Token);
            LaunchUpdater(launch, _pendingInstallations.Clear);
        }
        catch (OperationCanceledException)
        {
            SetModuleOperationStatus(_localize("Updates.PreparationCancelled", []));
        }
        catch (Exception exception)
        {
            SetModuleOperationStatus(_localize("Updates.PreparationFailed", []));
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(UpdateSearchScope.Modules, false);
        }
    }

    private async Task InstallAsync(UpdateSearchScope scope)
    {
        if (_preparationCancellation is not null) return;
        var isApplication = scope == UpdateSearchScope.Application;
        try
        {
            var result = isApplication
                ? await _applicationService.SearchAsync(_applicationSelections)
                : await _moduleService.SearchAsync(_moduleSelections);
            if (isApplication)
            {
                _applicationResult = result;
                RenderApplication(result);
            }
            else
            {
                _moduleResult = result;
                RenderModules(result);
            }
            if (!CanInstall(result)) return;
            if (MessageBox.Show(_owner, _localize("Updates.InstallConfirm", [result.Plan.Items.Count]),
                    _localize("Updates.Title", []), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(scope, true);
            var progressBar = isApplication ? _section.Progress : _section.ModulesProgress;
            var progress = new Progress<double>(value => progressBar.Value = value * 100);
            var launch = isApplication
                ? await _applicationService.PrepareAsync(result.Plan, progress, _preparationCancellation.Token)
                : await _packagePreparation.PrepareAsync(result.Plan, progress, _preparationCancellation.Token);
            LaunchUpdater(launch);
        }
        catch (OperationCanceledException)
        {
            SetStatus(scope, _localize("Updates.PreparationCancelled", []));
            if (scope == UpdateSearchScope.Application) _applicationState = UpdateVisualState.Neutral;
            else SetModuleVisualState(UpdateVisualState.Neutral);
        }
        catch (Exception exception)
        {
            SetStatus(scope, _localize("Updates.PreparationFailed", []));
            if (scope == UpdateSearchScope.Application) _applicationState = UpdateVisualState.Error;
            else SetModuleVisualState(UpdateVisualState.Error);
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(scope, false);
        }
    }

    private void LaunchUpdater(PreparedUpdateLaunch launch, Action? afterStart = null)
    {
        var start = new ProcessStartInfo(launch.UpdaterExecutable)
        {
            UseShellExecute = false,
            WorkingDirectory = launch.WorkingDirectory
        };
        start.ArgumentList.Add("--apply");
        start.ArgumentList.Add(launch.PlanPath);
        if (Process.Start(start) is null) throw new InvalidOperationException("The updater could not be started.");
        afterStart?.Invoke();
        _owner.Close();
        _ = Application.Current.Dispatcher.BeginInvoke(() => Application.Current.MainWindow?.Close());
    }

    private void Cancel(object sender, RoutedEventArgs e) => _preparationCancellation?.Cancel();

    private void SetPreparing(UpdateSearchScope scope, bool value)
    {
        if (value)
        {
            if (scope == UpdateSearchScope.Application) _applicationState = UpdateVisualState.Busy;
            else SetModuleVisualState(UpdateVisualState.Busy);
        }
        else if (scope == UpdateSearchScope.Application && _applicationState == UpdateVisualState.Busy)
            _applicationState = _applicationResult is null ? UpdateVisualState.Neutral : StateFor(_applicationResult);
        else if (scope == UpdateSearchScope.Modules)
            RestoreModuleVisualStateIfBusy();
        RebuildNavigation();
        _section.SearchButton.IsEnabled = !value;
        _section.SearchModulesButton.IsEnabled = !value;
        _section.SearchAvailableModulesButton.IsEnabled = !value;
        _section.InstallButton.IsEnabled = !value && _applicationResult is not null && CanInstall(_applicationResult);
        _section.InstallModulesButton.IsEnabled = !value && _moduleResult is not null && CanInstall(_moduleResult);
        _section.InstallModuleFileButton.IsEnabled = !value;
        _section.InstallModuleUrlButton.IsEnabled = !value;
        _section.ModuleCatalogUrl.IsEnabled = !value;
        _section.CancelButton.Visibility = value && scope == UpdateSearchScope.Application
            ? Visibility.Visible : Visibility.Collapsed;
        _section.CancelModulesButton.Visibility = value && scope == UpdateSearchScope.Modules
            ? Visibility.Visible : Visibility.Collapsed;
        _section.Progress.Visibility = value && scope == UpdateSearchScope.Application
            ? Visibility.Visible : Visibility.Collapsed;
        _section.ModulesProgress.Visibility = value && scope == UpdateSearchScope.Modules
            && _moduleOperationPage == ModuleOperationPage.Module ? Visibility.Visible : Visibility.Collapsed;
        _section.AvailableModulesProgress.Visibility = value && scope == UpdateSearchScope.Modules
            && _moduleOperationPage == ModuleOperationPage.Directory ? Visibility.Visible : Visibility.Collapsed;
        _section.ManualInstallationProgress.Visibility = value && scope == UpdateSearchScope.Modules
            && _moduleOperationPage == ModuleOperationPage.Advanced ? Visibility.Visible : Visibility.Collapsed;
        if (!value) return;
        if (scope == UpdateSearchScope.Application) _section.Progress.Value = 0;
        else ActiveModuleProgress().Value = 0;
        SetStatus(scope, _localize("Updates.Preparing", []));
    }

    private void SetStatus(UpdateSearchScope scope, string value)
    {
        if (scope == UpdateSearchScope.Application) _section.Status.Text = value;
        else SetModuleOperationStatus(value);
    }

    private ProgressBar ActiveModuleProgress() => _moduleOperationPage switch
    {
        ModuleOperationPage.Directory => _section.AvailableModulesProgress,
        ModuleOperationPage.Advanced => _section.ManualInstallationProgress,
        _ => _section.ModulesProgress
    };

    private void SetModuleOperationStatus(string value)
    {
        switch (_moduleOperationPage)
        {
            case ModuleOperationPage.Directory: _section.AvailableModuleStatus.Text = value; break;
            case ModuleOperationPage.Advanced: _section.ManualInstallationStatus.Text = value; break;
            default: _section.ModuleStatus.Text = value; break;
        }
    }

    private void SetModuleVisualState(UpdateVisualState state)
    {
        switch (_moduleOperationPage)
        {
            case ModuleOperationPage.Directory: _directoryState = state; break;
            case ModuleOperationPage.Advanced: _advancedState = state; break;
            default: _modulesState = state; break;
        }
    }

    private void RestoreModuleVisualStateIfBusy()
    {
        switch (_moduleOperationPage)
        {
            case ModuleOperationPage.Directory when _directoryState == UpdateVisualState.Busy:
                _directoryState = AvailableModuleResults.Count == 0 ? UpdateVisualState.Neutral : UpdateVisualState.Current;
                break;
            case ModuleOperationPage.Advanced when _advancedState == UpdateVisualState.Busy:
                _advancedState = UpdateVisualState.Neutral;
                break;
            case ModuleOperationPage.Module when _modulesState == UpdateVisualState.Busy:
                _modulesState = _moduleResult is null ? UpdateVisualState.Current : StateFor(_moduleResult);
                break;
        }
    }

    private static bool CanInstall(UpdateSearchResult result) =>
        result.Plan.IsCompatible && result.Plan.Items.Count > 0;

    private static void DeletePreparedWork(string workDirectory)
    {
        try { if (Directory.Exists(workDirectory)) Directory.Delete(workDirectory, recursive: true); }
        catch { }
    }

    private void PendingInstallationsChanged(object? sender, EventArgs e)
    {
        if (!_owner.Dispatcher.CheckAccess())
        {
            _owner.Dispatcher.Invoke(() => PendingInstallationsChanged(sender, e));
            return;
        }
        RefreshAvailableModuleRows();
    }

    private void RefreshAvailableModuleRows()
    {
        foreach (var row in AvailableModuleResults.ToArray())
        {
            var index = AvailableModuleResults.IndexOf(row);
            var downloaded = _pendingInstallations.Contains(row.Id);
            if (downloaded == row.IsDownloaded) continue;
            AvailableModuleResults[index] = new(row.Id, row.DisplayName, row.CatalogUrl,
                downloaded ? _localize("Updates.ModuleDownloaded", []) : row.DetailsLabel,
                row.IsInstalled, downloaded, !row.IsInstalled && !downloaded,
                downloaded ? _localize("Updates.ModuleDownloaded", [])
                    : _localize("Updates.ModuleDirectoryInstalledBadge", []));
        }
    }
}

internal enum ModuleOperationPage
{
    Directory,
    Module,
    Advanced
}

internal sealed class AvailableModuleRow
{
    internal AvailableModuleRow(string id, string displayName, string catalogUrl,
        string detailsLabel, bool isInstalled, bool isDownloaded, bool canInstall, string stateLabel)
    {
        Id = id;
        DisplayName = displayName;
        CatalogUrl = catalogUrl;
        DetailsLabel = detailsLabel;
        IsInstalled = isInstalled;
        IsDownloaded = isDownloaded;
        CanInstall = canInstall;
        StateLabel = stateLabel;
        var state = isInstalled || isDownloaded ? UpdateVisualState.Current
            : canInstall ? UpdateVisualState.Available : UpdateVisualState.Error;
        (StateForeground, StateBackground) = UpdateStateBrushes.For(state);
        StateIcon = StateIconFor(state);
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string CatalogUrl { get; }
    public string DetailsLabel { get; }
    public bool IsInstalled { get; }
    public bool IsDownloaded { get; }
    public bool ShowsStateBadge => IsInstalled || IsDownloaded;
    public bool CanInstall { get; }
    public string StateLabel { get; }
    public System.Windows.Media.Brush StateForeground { get; }
    public System.Windows.Media.Brush StateBackground { get; }
    public string StateIcon { get; }

    private static string StateIconFor(UpdateVisualState state) => state switch
    {
        UpdateVisualState.Current => "\uE73E",
        UpdateVisualState.Available => "\uE895",
        UpdateVisualState.Busy => "\uE895",
        UpdateVisualState.Error => "\uEA39",
        _ => "\uE7FC"
    };
}

internal sealed class UpdateComponentRow
{
    internal UpdateComponentRow(string id, UpdateComponentKind kind, string displayName, string installedLabel,
        string statusLabel, IReadOnlyList<string> versions, string? selectedVersion, bool canSelectVersion,
        UpdateVisualState state)
    {
        Id = id;
        Kind = kind;
        DisplayName = displayName;
        InstalledLabel = installedLabel;
        StatusLabel = statusLabel;
        Versions = versions;
        SelectedVersion = selectedVersion;
        CanSelectVersion = canSelectVersion;
        (StateForeground, StateBackground) = UpdateStateBrushes.For(state);
        StateIcon = state switch
        {
            UpdateVisualState.Current => "\uE73E",
            UpdateVisualState.Available => "\uE895",
            UpdateVisualState.Busy => "\uE895",
            UpdateVisualState.Error => "\uEA39",
            _ => "\uE7FC"
        };
    }

    public string Id { get; }
    public UpdateComponentKind Kind { get; }
    public string DisplayName { get; }
    public string InstalledLabel { get; }
    public string StatusLabel { get; }
    public IReadOnlyList<string> Versions { get; }
    public string? SelectedVersion { get; set; }
    public bool CanSelectVersion { get; }
    public System.Windows.Media.Brush StateForeground { get; }
    public System.Windows.Media.Brush StateBackground { get; }
    public string StateIcon { get; }
}
