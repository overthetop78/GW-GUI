using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.App.Controls;

public sealed class AtariCoreInstallationChangedEventArgs(AtariCoreKind kind, AtariCoreInstallationPaths paths)
    : EventArgs
{
    public AtariCoreKind Kind { get; } = kind;
    public AtariCoreInstallationPaths Paths { get; } = paths;
}

public sealed class AtariCoreManagementSection : UserControl
{
    private static readonly HttpClient Client = new();
    private readonly IAtariCoreReleaseService _service;
    private readonly Func<string, object[], string> _localize;
    private readonly EmulationCoreManagementPanel _view;
    private CancellationTokenSource? _downloadCancellation;
    private AtariCoreKind _kind;
    private string _requiredText = string.Empty;

    public event EventHandler<AtariCoreInstallationChangedEventArgs>? InstallationChanged;

    public AtariCoreManagementSection() : this(
        new AtariCoreReleaseService(Client, StoragePaths.AtariCoreDirectory),
        static (key, arguments) => LocExtension.Get(key, arguments))
    {
    }

    internal AtariCoreManagementSection(IAtariCoreReleaseService service,
        Func<string, object[], string> localize)
    {
        _service = service;
        _localize = localize;
        _view = new EmulationCoreManagementPanel(localize);
        _view.Versions.Name = "CoreVersions";
        _view.Versions.DisplayMemberPath = nameof(AtariCoreRelease.DeclaredVersion);
        _view.Search.Name = "SearchCoreVersions";
        _view.Download.Name = "DownloadCoreVersion";
        _view.Cancel.Name = "CancelCoreDownload";
        _view.Progress.Name = "CoreDownloadProgress";
        _view.Status.Name = "CoreStatus";
        AtariAccessibilityFunctions.Configure(_view.Versions,
            L(AtariCoreManagementConstants.VersionsFoundResource, AtariCoreManagementConstants.FirstVersionIndex));
        AtariAccessibilityFunctions.Configure(_view.Search, L(AtariCoreManagementConstants.SearchResource));
        AtariAccessibilityFunctions.Configure(_view.Download, L(AtariCoreManagementConstants.DownloadResource));
        AtariAccessibilityFunctions.Configure(_view.Cancel, L(AtariCoreManagementConstants.CancelResource));
        AtariAccessibilityFunctions.Configure(_view.Progress, L(AtariCoreManagementConstants.DownloadingResource,
            string.Empty));
        AtariAccessibilityFunctions.Configure(_view.Status, L(AtariCoreManagementConstants.SearchResource));
        AutomationProperties.SetLiveSetting(_view.Status, AutomationLiveSetting.Assertive);
        Content = _view;
        _view.Search.Click += async (_, _) => await SearchAsync();
        _view.Download.Click += async (_, _) => await DownloadSelectedAsync();
        _view.Cancel.Click += (_, _) => _downloadCancellation?.Cancel();
    }

    internal AtariCoreKind RequiredKind => _kind;
    internal string RequiredText => _requiredText;
    internal string InstalledText => _view.Installed.Text;
    internal string StatusText => _view.Status.Text;
    internal Visibility VersionsVisibility => _view.Versions.Visibility;
    internal Visibility DownloadVisibility => _view.Download.Visibility;
    internal Visibility CancelVisibility => _view.Cancel.Visibility;
    internal Visibility ProgressVisibility => _view.Progress.Visibility;
    internal IReadOnlyList<AtariCoreRelease> Versions => _view.Versions.Items.Cast<AtariCoreRelease>().ToArray();
    internal void SelectVersion(int index) => _view.Versions.SelectedIndex = index;

    public async Task SetModelAsync(AtariMachineModel model, CancellationToken cancellationToken = default)
    {
        _kind = AtariCompatibilityCatalog.Get(model).Core;
        _requiredText = AtariCoreCatalog.Get(_kind).LibraryName
            + AtariCoreManagementConstants.DetailSeparator
            + AtariConfigurationCatalogFunctions.ModelName(model);
        HideResults();
        _view.SetStatus(string.Empty);
        _view.ShowPrompt(L("Emulation.Core.NameSearchPrompt"));
        await RefreshInstalledStateAsync(cancellationToken);
    }

    internal async Task SearchAsync(CancellationToken cancellationToken = default)
    {
        HideResults();
        _view.SetStatus(string.Empty);
        _view.ShowPrompt(L(AtariCoreManagementConstants.SearchingResource));
        _view.Search.IsEnabled = false;
        try
        {
            var releases = await _service.GetAvailableAsync(_kind, cancellationToken);
            if (releases.Count == 0)
            {
                _view.ShowPrompt(L(AtariCoreManagementConstants.NoneFoundResource));
                return;
            }
            _view.Versions.ItemsSource = releases;
            _view.Versions.SelectedIndex = AtariCoreManagementConstants.FirstVersionIndex;
            _view.RequiredVersion.Text = releases[AtariCoreManagementConstants.FirstVersionIndex].DeclaredVersion;
            _view.LatestVersion.Text = releases[^1].DeclaredVersion;
            _view.FoundCount.Text = L(AtariCoreManagementConstants.VersionsFoundResource, releases.Count);
            _view.ShowResults();
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            var description = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariCoreManagement);
            _view.SetStatus(description, isError: true);
            _view.ShowPrompt(description);
        }
        finally
        {
            _view.Search.IsEnabled = true;
        }
    }

    internal async Task DownloadSelectedAsync()
    {
        if (_view.Versions.SelectedItem is not AtariCoreRelease release) return;
        _downloadCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _downloadCancellation = cancellation;
        SetBusy(true);
        _view.SetStatus(L(AtariCoreManagementConstants.DownloadingResource, release.DeclaredVersion));
        var progress = AtariCoreManagementFunctions.CreateDispatcherProgress<AtariCoreInstallProgress>(
            Dispatcher, value =>
            {
                if (value.Fraction is { } fraction) _view.Progress.Value = fraction;
            });
        try
        {
            var paths = await _service.InstallAsync(release, progress, cancellation.Token);
            await RefreshInstalledStateAsync(CancellationToken.None);
            _view.SetStatus(L(AtariCoreManagementConstants.InstalledPathResource, paths.LibraryPath));
            InstallationChanged?.Invoke(this, new AtariCoreInstallationChangedEventArgs(release.Kind, paths));
        }
        catch (OperationCanceledException)
        {
            _view.SetStatus(L(AtariCoreManagementConstants.CancelledResource));
        }
        catch (Exception error)
        {
            _view.SetStatus(ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariCoreManagement),
                isError: true);
        }
        finally
        {
            SetBusy(false);
            if (ReferenceEquals(_downloadCancellation, cancellation))
            {
                _downloadCancellation.Dispose();
                _downloadCancellation = null;
            }
        }
    }

    private async Task RefreshInstalledStateAsync(CancellationToken cancellationToken)
    {
        var paths = await _service.GetActiveInstallationAsync(_kind, cancellationToken);
        _view.Installed.Text = paths is null
            ? L(AtariCoreManagementConstants.NotInstalledResource)
            : L(AtariCoreManagementConstants.InstalledResource, Path.GetFileName(paths.VersionDirectory));
    }

    private void HideResults()
    {
        _view.Versions.ItemsSource = null;
        _view.Versions.Visibility = Visibility.Collapsed;
        _view.Download.Visibility = Visibility.Collapsed;
        _view.Results.Visibility = Visibility.Hidden;
    }

    private void SetBusy(bool busy)
    {
        _view.Search.IsEnabled = !busy;
        _view.Download.IsEnabled = !busy;
        _view.Cancel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        _view.Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (busy) _view.Progress.Value = AtariCoreManagementConstants.InitialProgress;
    }

    private string L(string key, params object[] arguments) => _localize(key, arguments);
}
