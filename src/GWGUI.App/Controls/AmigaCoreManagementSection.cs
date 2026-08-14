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
    private readonly ComboBox _versions = new() { Visibility = Visibility.Collapsed, MinWidth = 360 };
    private readonly Button _search = new() { MinWidth = 130 };
    private readonly Button _download = new() { MinWidth = 160, Visibility = Visibility.Collapsed };
    private readonly ProgressBar _progress = new() { Height = 8, Minimum = 0, Maximum = 1, Visibility = Visibility.Collapsed };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _requiredVersion = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _latestVersion = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _foundCount = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Grid _availableVersions = new() { Visibility = Visibility.Collapsed };
    private readonly Grid _releaseDetails = new() { Visibility = Visibility.Collapsed };

    public AmigaCoreManagementSection()
    {
        _search.Content = L("Emulation.CoreSearch");
        _download.Content = L("Emulation.CoreDownload");

        var panel = new Grid { VerticalAlignment = VerticalAlignment.Top };
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var installedRow = new Grid { Margin = new Thickness(8, 8, 8, 6) };
        installedRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
        installedRow.ColumnDefinitions.Add(new ColumnDefinition());
        installedRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        installedRow.Children.Add(new TextBlock
        {
            Text = L("Emulation.CoreProjectVersion"), FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0)
        });
        _installed.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_installed, 1);
        installedRow.Children.Add(_installed);
        _search.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(_search, 2);
        installedRow.Children.Add(_search);
        panel.Children.Add(installedRow);

        _availableVersions.ColumnDefinitions.Add(new ColumnDefinition());
        _availableVersions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _availableVersions.Margin = new Thickness(8, 6, 8, 6);
        _versions.Margin = new Thickness(0, 0, 12, 0);
        _availableVersions.Children.Add(_versions);
        Grid.SetColumn(_download, 1);
        _availableVersions.Children.Add(_download);
        Grid.SetRow(_availableVersions, 1);
        panel.Children.Add(_availableVersions);

        var details = _releaseDetails;
        details.Margin = new Thickness(8, 6, 8, 6);
        details.ColumnDefinitions.Add(new ColumnDefinition());
        details.ColumnDefinitions.Add(new ColumnDefinition());
        details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddDetail(details, 0, L("Emulation.CoreRequiredVersion"), _requiredVersion);
        AddDetail(details, 1, L("Emulation.CoreLatestVersion"), _latestVersion);
        Grid.SetRow(_foundCount, 1);
        Grid.SetColumnSpan(_foundCount, 2);
        _foundCount.Margin = new Thickness(0, 8, 0, 0);
        details.Children.Add(_foundCount);
        Grid.SetRow(details, 2);
        panel.Children.Add(details);

        _status.Margin = new Thickness(8, 4, 8, 8);
        Grid.SetRow(_status, 3);
        panel.Children.Add(_status);
        Content = panel;

        _search.Click += async (_, _) => await SearchAsync();
        _download.Click += async (_, _) => await DownloadAsync();
        _versions.SelectionChanged += (_, _) => _versions.ToolTip = (_versions.SelectedItem as AmigaCoreRelease)?.DisplayName;
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
            _availableVersions.Visibility = Visibility.Visible;
            _releaseDetails.Visibility = Visibility.Visible;
            var required = releases.First(release => release.IsRequired);
            var latest = releases.Last();
            _requiredVersion.Text = required.DisplayName;
            _latestVersion.Text = latest.DisplayName;
            _foundCount.Text = L("Emulation.CoreVersionsFound", releases.Count);
            _status.Text = string.Empty;
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

    private static void AddDetail(Grid grid, int column, string label, TextBlock value)
    {
        var block = new StackPanel { Margin = new Thickness(column == 0 ? 0 : 12, 0, column == 0 ? 12 : 0, 0) };
        block.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
        block.Children.Add(value);
        Grid.SetColumn(block, column);
        grid.Children.Add(block);
    }

}
