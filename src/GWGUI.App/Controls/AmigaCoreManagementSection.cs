using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

public sealed class AmigaCoreManagementSection : UserControl
{
    private static readonly HttpClient Client = new();
    private readonly AmigaCoreReleaseService _service = new(Client, StoragePaths.AmigaCoreDirectory);
    private readonly TextBlock _installed = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly Border _installedBadge = new() { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1) };
    private readonly ComboBox _versions = new() { MinWidth = 320 };
    private readonly Button _search = new() { MinWidth = 130 };
    private readonly Button _download = new() { MinWidth = 160, Visibility = Visibility.Collapsed };
    private readonly ProgressBar _progress = new() { Height = 8, Minimum = 0, Maximum = 1, Visibility = Visibility.Collapsed };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Border _statusBanner = new()
    {
        CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
        Padding = new Thickness(10, 7, 10, 7), Visibility = Visibility.Collapsed
    };
    private readonly TextBlock _requiredVersion = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock _latestVersion = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock _foundCount = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly Grid _results = new() { Visibility = Visibility.Hidden };
    private readonly TextBlock _prompt = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Border _promptBanner = new()
    {
        CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
        Padding = new Thickness(14, 10, 14, 10), HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };

    public AmigaCoreManagementSection()
    {
        _search.Content = ButtonContent("\uE721", L("Emulation.CoreSearch"));
        _download.Content = ButtonContent("\uE896", L("Emulation.CoreDownload"));

        var panel = new Grid { VerticalAlignment = VerticalAlignment.Top };
        panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(62) });
        panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(106) });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var installedRow = new Grid { Margin = new Thickness(16, 10, 16, 6) };
        installedRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        installedRow.ColumnDefinitions.Add(new ColumnDefinition());
        installedRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var title = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 24, 0) };
        title.Children.Add(new TextBlock
        {
            Text = ControlVisualConstants.GameControllerGlyph, FontFamily = ControlVisualConstants.IconFont, FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0)
        });
        title.Children.Add(new TextBlock
        {
            Text = L("Emulation.Emulator"), FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });
        installedRow.Children.Add(title);
        var installedPanel = new StackPanel
        {
            Margin = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center
        };
        var installedLabel = new TextBlock
        {
            Text = L("Emulation.CoreProjectVersion"), FontSize = 11,
            Margin = new Thickness(0, 0, 0, 2)
        };
        installedLabel.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        installedPanel.Children.Add(installedLabel);
        _installed.VerticalAlignment = VerticalAlignment.Center;
        _installed.FontWeight = FontWeights.SemiBold;
        installedPanel.Children.Add(_installed);
        _installedBadge.Child = installedPanel;
        _installedBadge.Padding = new Thickness(12, 6, 12, 6);
        _installedBadge.SetResourceReference(BackgroundProperty, "WindowBrush");
        _installedBadge.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Grid.SetColumn(_installedBadge, 1);
        installedRow.Children.Add(_installedBadge);
        _search.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(_search, 2);
        installedRow.Children.Add(_search);
        panel.Children.Add(installedRow);

        var resultHost = new Grid { Margin = new Thickness(16, 4, 16, 2) };
        _prompt.Text = L("Emulation.CoreSearchPrompt");
        _prompt.VerticalAlignment = VerticalAlignment.Center;
        _prompt.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        var promptContent = new StackPanel { Orientation = Orientation.Horizontal };
        var promptIcon = new TextBlock
        {
            Text = ControlVisualConstants.InformationGlyph, FontFamily = ControlVisualConstants.IconFont, FontSize = 17,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 9, 0)
        };
        promptIcon.SetResourceReference(ForegroundProperty, "AccentBrush");
        promptContent.Children.Add(promptIcon);
        promptContent.Children.Add(_prompt);
        _promptBanner.Child = promptContent;
        _promptBanner.SetResourceReference(BackgroundProperty, "WindowBrush");
        _promptBanner.SetResourceReference(BorderBrushProperty, "BorderBrush");
        resultHost.Children.Add(_promptBanner);

        _results.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
        _results.RowDefinitions.Add(new RowDefinition());
        var availableVersions = new Grid();
        availableVersions.ColumnDefinitions.Add(new ColumnDefinition());
        availableVersions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _versions.Margin = new Thickness(0, 0, 12, 0);
        availableVersions.Children.Add(_versions);
        Grid.SetColumn(_download, 1);
        availableVersions.Children.Add(_download);
        _results.Children.Add(availableVersions);

        var details = new Grid { Margin = new Thickness(0, 7, 0, 0) };
        details.ColumnDefinitions.Add(new ColumnDefinition());
        details.ColumnDefinitions.Add(new ColumnDefinition());
        details.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        details.Children.Add(DetailTile(L("Emulation.CoreRequiredVersion"), _requiredVersion,
            new Thickness(0, 0, 5, 0)));
        var latestTile = DetailTile(L("Emulation.CoreLatestVersion"), _latestVersion,
            new Thickness(5, 0, 5, 0));
        Grid.SetColumn(latestTile, 1);
        details.Children.Add(latestTile);
        _foundCount.VerticalAlignment = VerticalAlignment.Center;
        _foundCount.HorizontalAlignment = HorizontalAlignment.Center;
        _foundCount.FontWeight = FontWeights.SemiBold;
        var countTile = new Border
        {
            Child = _foundCount, CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(5, 0, 0, 0)
        };
        countTile.SetResourceReference(BackgroundProperty, "WindowBrush");
        countTile.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Grid.SetColumn(countTile, 2);
        details.Children.Add(countTile);
        Grid.SetRow(details, 1);
        _results.Children.Add(details);
        resultHost.Children.Add(_results);
        Grid.SetRow(resultHost, 1);
        panel.Children.Add(resultHost);

        var statusHost = new Grid { Margin = new Thickness(16, 0, 16, 6) };
        statusHost.RowDefinitions.Add(new RowDefinition());
        statusHost.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
        _status.VerticalAlignment = VerticalAlignment.Center;
        _statusBanner.Child = _status;
        statusHost.Children.Add(_statusBanner);
        Grid.SetRow(_progress, 1);
        statusHost.Children.Add(_progress);
        Grid.SetRow(statusHost, 2);
        panel.Children.Add(statusHost);

        var card = new Border
        {
            Child = panel,
            CornerRadius = new CornerRadius(9),
            BorderThickness = new Thickness(1)
        };
        card.SetResourceReference(BackgroundProperty, "CardBrush");
        card.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Content = card;

        _search.Click += async (_, _) => await SearchAsync();
        _download.Click += async (_, _) => await DownloadAsync();
        _versions.SelectionChanged += (_, _) => _versions.ToolTip = (_versions.SelectedItem as AmigaCoreRelease)?.DisplayName;
        Loaded += (_, _) => RefreshInstalledState();
    }

    private static string L(string key, params object[] arguments) => LocExtension.Get(key, arguments);

    private async Task SearchAsync()
    {
        await RunCoreActionAsync(_search, async () =>
        {
            SetStatus(string.Empty);
            _prompt.Text = L("Emulation.CoreSearching");
            var releases = await _service.GetAvailableAsync();
            if (releases.Count == 0)
            {
                _prompt.Text = L("Emulation.CoreNoneFound");
                _results.Visibility = Visibility.Hidden;
                _promptBanner.Visibility = Visibility.Visible;
                SetStatus(string.Empty);
                return;
            }
            _versions.ItemsSource = releases;
            var required = releases.FirstOrDefault(release => release.IsRequired) ?? releases[0];
            _versions.SelectedItem = required;
            _versions.Visibility = Visibility.Visible;
            _download.Visibility = Visibility.Visible;
            _results.Visibility = Visibility.Visible;
            _promptBanner.Visibility = Visibility.Hidden;
            var latest = releases.Last();
            _requiredVersion.Text = required.DisplayName;
            _latestVersion.Text = latest.DisplayName;
            _foundCount.Text = L("Emulation.CoreVersionsFound", releases.Count);
            SetStatus(string.Empty);
        });
    }

    private async Task DownloadAsync()
    {
        if (_versions.SelectedItem is not AmigaCoreRelease release) return;
        await RunCoreActionAsync(_download, async () =>
        {
            _progress.Value = 0;
            _progress.Visibility = Visibility.Visible;
            SetStatus(L("Emulation.CoreDownloading", release.DisplayName));
            var progress = new Progress<double>(value => _progress.Value = value);
            var path = await _service.InstallAsync(release, progress);
            SetStatus(L("Emulation.CoreInstalledPath", path));
            RefreshInstalledState();
        });
    }

    private Task RunCoreActionAsync(Button button, Func<Task> action) =>
        ButtonAsyncAction.RunAsync(button, action, error =>
        {
            SetStatus(ControlErrorPresenter.Describe(error, ControlErrorContexts.AmigaCoreManagement), isError: true);
            if (_results.Visibility != Visibility.Visible)
                _prompt.Text = _status.Text;
        }, () => _progress.Visibility = Visibility.Collapsed);

    private void RefreshInstalledState()
    {
        var version = _service.GetInstalledVersion();
        _installed.Text = version is null
            ? L("Emulation.CoreNotInstalled")
            : L("Emulation.CoreInstalled", version);
    }

    private void SetStatus(string text, bool isError = false)
    {
        _status.Text = text;
        _statusBanner.Visibility = string.IsNullOrWhiteSpace(text) ? Visibility.Collapsed : Visibility.Visible;
        if (isError)
        {
            _statusBanner.Background = new SolidColorBrush(Color.FromRgb(255, 241, 241));
            _statusBanner.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 75, 75));
            _status.Foreground = new SolidColorBrush(Color.FromRgb(150, 25, 25));
        }
        else
        {
            _statusBanner.SetResourceReference(BackgroundProperty, "WindowBrush");
            _statusBanner.SetResourceReference(BorderBrushProperty, "BorderBrush");
            _status.SetResourceReference(ForegroundProperty, "TextBrush");
        }
    }

    private static UIElement ButtonContent(string icon, string text)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(new TextBlock
        {
            Text = icon, FontFamily = ControlVisualConstants.IconFont, FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0)
        });
        panel.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
        return panel;
    }

    private static Border DetailTile(string label, TextBlock value, Thickness margin)
    {
        var content = new StackPanel();
        var caption = new TextBlock
        {
            Text = label, FontSize = 11, Margin = new Thickness(0, 0, 0, 2),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        caption.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        content.Children.Add(caption);
        value.FontWeight = FontWeights.SemiBold;
        content.Children.Add(value);
        var tile = new Border
        {
            Child = content, CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 7, 10, 7), Margin = margin
        };
        tile.SetResourceReference(BackgroundProperty, "WindowBrush");
        tile.SetResourceReference(BorderBrushProperty, "BorderBrush");
        return tile;
    }

}
