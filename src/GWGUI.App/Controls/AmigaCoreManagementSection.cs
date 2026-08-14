using System.Diagnostics;
using System.IO;
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
    private readonly TextBlock _required = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _latest = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _installed = new() { TextWrapping = TextWrapping.Wrap };
    private readonly ComboBox _versions = new() { Visibility = Visibility.Collapsed, MinWidth = 420 };
    private readonly Button _search = new() { MinWidth = 190 };
    private readonly Button _download = new() { MinWidth = 240, Visibility = Visibility.Collapsed };
    private readonly Button _openFolder = new() { MinWidth = 150 };
    private readonly ProgressBar _progress = new() { Height = 8, Minimum = 0, Maximum = 1, Visibility = Visibility.Collapsed };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };

    public AmigaCoreManagementSection()
    {
        _required.Text = AmigaCoreReleaseService.RequiredDisplayName;
        _latest.Text = L("Emulation.CoreSearchPrompt");
        _search.Content = L("Emulation.CoreSearch");
        _download.Content = L("Emulation.CoreDownload");
        _openFolder.Content = L("Common.OpenFolder");

        var panel = new StackPanel { Margin = new Thickness(18), MaxWidth = 900, HorizontalAlignment = HorizontalAlignment.Left };
        panel.Children.Add(new TextBlock { Text = L("Emulation.CoreManagerTitle", "Amiga"), FontSize = 20, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock
        {
            Text = L("Emulation.CoreManagerDescription"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 18)
        });
        AddInformation(panel, L("Emulation.CoreRequiredVersion"), _required);
        AddInformation(panel, L("Emulation.CoreLatestVersion"), _latest);
        AddInformation(panel, L("Emulation.CoreProjectVersion"), _installed);

        var actions = new WrapPanel { Margin = new Thickness(0, 8, 0, 8) };
        foreach (var button in new[] { _search, _openFolder })
        {
            button.Margin = new Thickness(0, 0, 8, 8);
            actions.Children.Add(button);
        }
        panel.Children.Add(actions);
        _versions.Margin = new Thickness(0, 4, 0, 8);
        panel.Children.Add(_versions);
        _download.Margin = new Thickness(0, 0, 0, 8);
        _download.HorizontalAlignment = HorizontalAlignment.Left;
        panel.Children.Add(_download);
        panel.Children.Add(_progress);
        _status.Margin = new Thickness(0, 8, 0, 0);
        panel.Children.Add(_status);
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };

        _search.Click += async (_, _) => await SearchAsync();
        _download.Click += async (_, _) => await DownloadAsync();
        _openFolder.Click += (_, _) => OpenFolder();
        Loaded += (_, _) => RefreshInstalledState();
    }

    private static string L(string key, params object[] arguments) => LocExtension.Get(key, arguments);

    private static void AddInformation(Panel panel, string title, UIElement value)
    {
        panel.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 3) });
        panel.Children.Add(value);
        if (value is FrameworkElement element) element.Margin = new Thickness(0, 0, 0, 14);
    }

    private async Task SearchAsync()
    {
        await RunAsync(_search, async () =>
        {
            _status.Text = L("Emulation.CoreSearching");
            var releases = await _service.GetAvailableAsync();
            _versions.ItemsSource = releases;
            _versions.SelectedItem = releases.First(release => release.IsRequired);
            _versions.Visibility = Visibility.Visible;
            _download.Visibility = Visibility.Visible;
            var latest = releases.Where(release => !release.IsRequired)
                .OrderByDescending(release => release.PublishedUtc).FirstOrDefault();
            _latest.Text = latest?.DisplayName ?? L("Emulation.CoreNoneFound");
            _status.Text = L("Emulation.CoreVersionsFound", releases.Count);
        });
    }

    private async Task DownloadAsync()
    {
        if (_versions.SelectedItem is not AmigaCoreRelease release) return;
        await RunAsync(_download, async () =>
        {
            _progress.Value = 0;
            _progress.Visibility = Visibility.Visible;
            _status.Text = L("Emulation.CoreDownloading", release.DisplayName);
            var progress = new Progress<double>(value => _progress.Value = value);
            var path = await _service.InstallAsync(release, progress);
            _status.Text = L("Emulation.CoreInstalledPath", path);
            RefreshInstalledState();
        });
    }

    private async Task RunAsync(Button button, Func<Task> action)
    {
        try
        {
            button.IsEnabled = false;
            await action();
        }
        catch (Exception error)
        {
            var path = ErrorLog.Write(error, "Managing the external Amiga core");
            var detail = path is null ? L("Common.Unknown") : L("Error.LogSaved", path);
            _status.Text = L("Error.Unexpected", detail);
        }
        finally
        {
            button.IsEnabled = true;
            _progress.Visibility = Visibility.Collapsed;
        }
    }

    private void RefreshInstalledState()
    {
        var version = _service.GetInstalledVersion();
        _installed.Text = version is null
            ? L("Emulation.CoreNotInstalled")
            : L("Emulation.CoreInstalled", version);
    }

    private static void OpenFolder()
    {
        Directory.CreateDirectory(StoragePaths.AmigaCoreDirectory);
        Process.Start(new ProcessStartInfo(StoragePaths.AmigaCoreDirectory) { UseShellExecute = true });
    }
}
