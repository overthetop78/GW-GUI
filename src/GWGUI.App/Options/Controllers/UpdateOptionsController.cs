using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.App.Services.Updates;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Updates.Contracts;

namespace GWGUI.App.Options.Controllers;

internal sealed class UpdateOptionsController
{
    private readonly OptionsUpdatesSection _section;
    private readonly Window _owner;
    private readonly ApplicationUpdateService _applicationService;
    private readonly ModuleUpdateService _moduleService;
    private readonly ModuleDirectoryService _moduleDirectoryService;
    private readonly UpdatePackagePreparationService _packagePreparation;
    private readonly ModuleInstallationService _moduleInstallation;
    private readonly Func<string, object[], string> _localize;
    private readonly Action<Exception> _reportError;
    private readonly Dictionary<string, string> _applicationSelections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _moduleSelections = new(StringComparer.OrdinalIgnoreCase);
    private UpdateSearchResult? _applicationResult;
    private UpdateSearchResult? _moduleResult;
    private CancellationTokenSource? _preparationCancellation;

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
        ModuleInstallationService? moduleInstallation = null)
    {
        _owner = owner;
        _section = section;
        _applicationService = applicationService;
        _moduleService = moduleService ?? new ModuleUpdateService();
        _moduleDirectoryService = moduleDirectoryService ?? new ModuleDirectoryService();
        _packagePreparation = packagePreparation ?? new UpdatePackagePreparationService();
        _moduleInstallation = moduleInstallation ?? new ModuleInstallationService(
            packagePreparation: _packagePreparation);
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
        RefreshLocalizedContent();
    }

    internal void RefreshLocalizedContent()
    {
        if (_applicationResult is null) _section.Status.Text = _localize("Updates.Ready", []);
        else RenderApplication(_applicationResult);
        if (_moduleResult is null) _section.ModuleStatus.Text = _localize("Updates.Ready", []);
        else RenderModules(_moduleResult);
        if (AvailableModuleResults.Count == 0)
            _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectoryReady", []);
    }

    private async void SearchApplication(object sender, RoutedEventArgs e)
    {
        _section.SearchButton.IsEnabled = false;
        _section.Status.Text = _localize("Updates.Searching", []);
        try
        {
            _applicationResult = await _applicationService.SearchAsync(_applicationSelections);
            RenderApplication(_applicationResult);
        }
        catch (Exception exception)
        {
            _section.Status.Text = _localize("Updates.SearchFailed", []);
            _reportError(exception);
        }
        finally { if (_preparationCancellation is null) _section.SearchButton.IsEnabled = true; }
    }

    private async void SearchModules(object sender, RoutedEventArgs e)
    {
        _section.SearchModulesButton.IsEnabled = false;
        _section.ModuleStatus.Text = _localize("Updates.Searching", []);
        try
        {
            _moduleResult = await _moduleService.SearchAsync(_moduleSelections);
            RenderModules(_moduleResult);
        }
        catch (Exception exception)
        {
            _section.ModuleStatus.Text = _localize("Updates.SearchFailed", []);
            _reportError(exception);
        }
        finally { if (_preparationCancellation is null) _section.SearchModulesButton.IsEnabled = true; }
    }

    private async void SearchAvailableModules(object sender, RoutedEventArgs e)
    {
        _section.SearchAvailableModulesButton.IsEnabled = false;
        _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectorySearching", []);
        try
        {
            var modules = await _moduleDirectoryService.SearchAsync();
            AvailableModuleResults.Clear();
            foreach (var module in modules)
            {
                var state = module.InstalledVersion is not null
                    ? _localize("Updates.InstalledVersion", [module.InstalledVersion])
                    : module.AvailableVersion is null
                        ? _localize("Updates.Incompatible", [])
                        : _localize("Updates.ModuleNotInstalled", []);
                var version = module.AvailableVersion is null
                    ? _localize("Updates.Incompatible", [])
                    : _localize("Updates.ModuleAvailableVersion", [module.AvailableVersion]);
                AvailableModuleResults.Add(new(module.Id, module.DisplayName, module.CatalogUrl,
                    version, state, module.CanInstall));
            }
            _section.AvailableModuleStatus.Text = AvailableModuleResults.Count == 0
                ? _localize("Updates.ModuleDirectoryEmpty", [])
                : _localize("Updates.ModuleDirectoryResults", [AvailableModuleResults.Count]);
        }
        catch (Exception exception)
        {
            _section.AvailableModuleStatus.Text = _localize("Updates.ModuleDirectoryFailed", []);
            _reportError(exception);
        }
        finally
        {
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
                update.Availability == UpdateAvailability.Available));
        }
    }

    private string ResultStatus(UpdateSearchResult result) =>
        result.Components.Any(update => update.Availability != UpdateAvailability.UpToDate)
            ? _localize("Updates.Results", []) : _localize("Updates.NoUpdates", []);

    private async void InstallApplication(object sender, RoutedEventArgs e) =>
        await InstallAsync(UpdateSearchScope.Application);

    private async void InstallModules(object sender, RoutedEventArgs e) =>
        await InstallAsync(UpdateSearchScope.Modules);

    private async void InstallModuleFromFile(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = _localize("Updates.ModuleInstallFromFile", []),
            Filter = _localize("Updates.ModuleArchiveFilter", [])
        };
        if (dialog.ShowDialog(_owner) != true) return;
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromFileAsync(dialog.FileName, cancellation));
    }

    private async void InstallModuleFromUrl(object sender, RoutedEventArgs e)
    {
        var catalogUrl = _section.ModuleCatalogUrl.Text.Trim();
        if (catalogUrl.Length == 0)
        {
            _section.ModuleStatus.Text = _localize("Updates.ModuleCatalogUrlRequired", []);
            return;
        }
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromCatalogAsync(catalogUrl,
                new Progress<double>(value => _section.ModulesProgress.Value = value * 100), cancellation));
    }

    private async void InstallAvailableModule(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: AvailableModuleRow module } || !module.CanInstall) return;
        await PrepareNewModuleAsync(cancellation =>
            _moduleInstallation.PrepareFromCatalogAsync(module.CatalogUrl,
                new Progress<double>(value => _section.ModulesProgress.Value = value * 100), cancellation));
    }

    private async Task PrepareNewModuleAsync(
        Func<CancellationToken, Task<PreparedModuleInstallation>> prepare)
    {
        if (_preparationCancellation is not null) return;
        PreparedModuleInstallation? prepared = null;
        try
        {
            _preparationCancellation = new CancellationTokenSource();
            SetPreparing(UpdateSearchScope.Modules, true);
            prepared = await prepare(_preparationCancellation.Token);
            var confirmation = _localize("Updates.ModuleInstallConfirm",
                [prepared.ModuleId, prepared.ModuleVersion]);
            if (MessageBox.Show(_owner, confirmation, _localize("Updates.Title", []),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                DeletePreparedWork(prepared.Launch.WorkingDirectory);
                return;
            }
            LaunchUpdater(prepared.Launch);
        }
        catch (OperationCanceledException)
        {
            _section.ModuleStatus.Text = _localize("Updates.PreparationCancelled", []);
        }
        catch (Exception exception)
        {
            if (prepared is not null) DeletePreparedWork(prepared.Launch.WorkingDirectory);
            _section.ModuleStatus.Text = _localize("Updates.ModuleInstallFailed", []);
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
        }
        catch (Exception exception)
        {
            SetStatus(scope, _localize("Updates.PreparationFailed", []));
            _reportError(exception);
        }
        finally
        {
            _preparationCancellation?.Dispose();
            _preparationCancellation = null;
            if (_owner.IsVisible) SetPreparing(scope, false);
        }
    }

    private void LaunchUpdater(PreparedUpdateLaunch launch)
    {
        var start = new ProcessStartInfo(launch.UpdaterExecutable)
        {
            UseShellExecute = false,
            WorkingDirectory = launch.WorkingDirectory
        };
        start.ArgumentList.Add("--apply");
        start.ArgumentList.Add(launch.PlanPath);
        if (Process.Start(start) is null) throw new InvalidOperationException("The updater could not be started.");
        _owner.Close();
        _ = Application.Current.Dispatcher.BeginInvoke(() => Application.Current.MainWindow?.Close());
    }

    private void Cancel(object sender, RoutedEventArgs e) => _preparationCancellation?.Cancel();

    private void SetPreparing(UpdateSearchScope scope, bool value)
    {
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
            ? Visibility.Visible : Visibility.Collapsed;
        if (!value) return;
        if (scope == UpdateSearchScope.Application) _section.Progress.Value = 0;
        else _section.ModulesProgress.Value = 0;
        SetStatus(scope, _localize("Updates.Preparing", []));
    }

    private void SetStatus(UpdateSearchScope scope, string value)
    {
        if (scope == UpdateSearchScope.Application) _section.Status.Text = value;
        else _section.ModuleStatus.Text = value;
    }

    private static bool CanInstall(UpdateSearchResult result) =>
        result.Plan.IsCompatible && result.Plan.Items.Count > 0;

    private static void DeletePreparedWork(string workDirectory)
    {
        try { if (Directory.Exists(workDirectory)) Directory.Delete(workDirectory, recursive: true); }
        catch { }
    }
}

internal sealed class AvailableModuleRow
{
    internal AvailableModuleRow(string id, string displayName, string catalogUrl,
        string versionLabel, string stateLabel, bool canInstall)
    {
        Id = id;
        DisplayName = displayName;
        CatalogUrl = catalogUrl;
        VersionLabel = versionLabel;
        StateLabel = stateLabel;
        CanInstall = canInstall;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string CatalogUrl { get; }
    public string VersionLabel { get; }
    public string StateLabel { get; }
    public bool CanInstall { get; }
}

internal sealed class UpdateComponentRow
{
    internal UpdateComponentRow(string id, UpdateComponentKind kind, string displayName, string installedLabel,
        string statusLabel, IReadOnlyList<string> versions, string? selectedVersion, bool canSelectVersion)
    {
        Id = id;
        Kind = kind;
        DisplayName = displayName;
        InstalledLabel = installedLabel;
        StatusLabel = statusLabel;
        Versions = versions;
        SelectedVersion = selectedVersion;
        CanSelectVersion = canSelectVersion;
    }

    public string Id { get; }
    public UpdateComponentKind Kind { get; }
    public string DisplayName { get; }
    public string InstalledLabel { get; }
    public string StatusLabel { get; }
    public IReadOnlyList<string> Versions { get; }
    public string? SelectedVersion { get; set; }
    public bool CanSelectVersion { get; }
}
