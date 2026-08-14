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
    private readonly TextBlock _installed = new() { TextWrapping = TextWrapping.Wrap };
    private readonly ComboBox _versions = new() { Visibility = Visibility.Collapsed, MinWidth = 300 };
    private readonly Button _search = new() { MinWidth = 130 };
    private readonly Button _download = new() { MinWidth = 130, Visibility = Visibility.Collapsed };
    private readonly ProgressBar _progress = new() { Height = 8, Minimum = 0, Maximum = 1, Visibility = Visibility.Collapsed };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };

    public AmigaCoreManagementSection()
    {
        _search.Content = L("Emulation.CoreSearch");
        _download.Content = L("Emulation.CoreDownload");

        var panel = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock
        {
            Text = L("Emulation.CoreManagerTitle", "Amiga"),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        });
        _installed.Margin = new Thickness(0, 0, 12, 0);
        _installed.VerticalAlignment = VerticalAlignment.Center;
        panel.Children.Add(_installed);
        _search.Margin = new Thickness(0, 0, 8, 0);
        panel.Children.Add(_search);
        _versions.Margin = new Thickness(0, 0, 8, 0);
        panel.Children.Add(_versions);
        panel.Children.Add(_download);
        _status.Margin = new Thickness(12, 0, 0, 0);
        _status.VerticalAlignment = VerticalAlignment.Center;
        panel.Children.Add(_status);
        Content = panel;

        _search.Click += async (_, _) => await SearchAsync();
        _download.Click += async (_, _) => await DownloadAsync();
        Loaded += (_, _) => RefreshInstalledState();
    }

    private static string L(string key, params object[] arguments) => LocExtension.Get(key, arguments);

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

}
