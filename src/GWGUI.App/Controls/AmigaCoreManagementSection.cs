using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

public sealed class AmigaCoreManagementSection : UserControl
{
    private static readonly HttpClient Client = new();
    private readonly AmigaCoreReleaseService _service = new(Client, StoragePaths.AmigaCoreDirectory);
    private readonly EmulationCoreManagementPanel _view = new(
        static (key, arguments) => LocExtension.Get(key, arguments));

    public AmigaCoreManagementSection()
    {
        Content = _view;
        _view.ShowPrompt(L("Emulation.Core.NameSearchPrompt"));
        _view.Search.Click += async (_, _) => await SearchAsync();
        _view.Download.Click += async (_, _) => await DownloadAsync();
        _view.Versions.SelectionChanged += (_, _) =>
            _view.Versions.ToolTip = (_view.Versions.SelectedItem as AmigaCoreRelease)?.DisplayName;
        Loaded += (_, _) => RefreshInstalledState();
    }

    private static string L(string key, params object[] arguments) => LocExtension.Get(key, arguments);

    private async Task SearchAsync()
    {
        await RunCoreActionAsync(_view.Search, async () =>
        {
            _view.SetStatus(string.Empty);
            _view.ShowPrompt(L("Emulation.Core.NameSearching"));
            var releases = await _service.GetAvailableAsync();
            if (releases.Count == 0)
            {
                _view.ShowPrompt(L("Emulation.Core.NameNoneFound"));
                return;
            }
            _view.Versions.ItemsSource = releases;
            var required = releases.FirstOrDefault(release => release.IsRequired) ?? releases[0];
            _view.Versions.SelectedItem = required;
            _view.RequiredVersion.Text = required.DisplayName;
            _view.LatestVersion.Text = releases.Last().DisplayName;
            _view.FoundCount.Text = L("Emulation.Core.NameVersionsFound", releases.Count);
            _view.ShowResults();
        });
    }

    private async Task DownloadAsync()
    {
        if (_view.Versions.SelectedItem is not AmigaCoreRelease release) return;
        await RunCoreActionAsync(_view.Download, async () =>
        {
            _view.Progress.Value = 0;
            _view.Progress.Visibility = Visibility.Visible;
            _view.SetStatus(L("Emulation.Core.NameDownloading", release.DisplayName));
            var path = await _service.InstallAsync(release,
                new Progress<double>(value => _view.Progress.Value = value));
            _view.SetStatus(L("Emulation.Core.NameInstalledPath", path));
            RefreshInstalledState();
        });
    }

    private Task RunCoreActionAsync(Button button, Func<Task> action) =>
        ButtonAsyncAction.RunAsync(button, action, error =>
        {
            var description = ControlErrorPresenter.Describe(error, ControlErrorContexts.AmigaCoreManagement);
            _view.SetStatus(description, isError: true);
            if (_view.Results.Visibility != Visibility.Visible) _view.ShowPrompt(description);
        }, () => _view.Progress.Visibility = Visibility.Collapsed);

    private void RefreshInstalledState()
    {
        var version = _service.GetInstalledVersion();
        _view.Installed.Text = version is null
            ? L("Emulation.Core.NameNotInstalled")
            : L("Emulation.Core.NameInstalled", version);
    }
}
