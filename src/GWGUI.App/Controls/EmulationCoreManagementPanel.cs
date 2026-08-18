using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Controls;

internal sealed class EmulationCoreManagementPanel : UserControl
{
    internal TextBlock Installed { get; } = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    internal ComboBox Versions { get; } = new() { MinWidth = 320 };
    internal Button Search { get; } = new() { MinWidth = 130 };
    internal Button Download { get; } = new() { MinWidth = 160, Visibility = Visibility.Collapsed };
    internal Button Cancel { get; } = new() { MinWidth = 100, Visibility = Visibility.Collapsed };
    internal ProgressBar Progress { get; } = new() { Height = 8, Minimum = 0, Maximum = 1, Visibility = Visibility.Collapsed };
    internal TextBlock Status { get; } = new() { TextWrapping = TextWrapping.Wrap };
    internal TextBlock RequiredVersion { get; } = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    internal TextBlock LatestVersion { get; } = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    internal TextBlock FoundCount { get; } = new() { TextTrimming = TextTrimming.CharacterEllipsis };
    internal Grid Results { get; } = new() { Visibility = Visibility.Hidden };
    internal TextBlock Prompt { get; } = new() { TextWrapping = TextWrapping.Wrap };

    private readonly Border _statusBanner = new()
    {
        CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
        Padding = new Thickness(10, 7, 10, 7), Visibility = Visibility.Collapsed
    };
    private readonly Border _promptBanner = new()
    {
        CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
        Padding = new Thickness(14, 10, 14, 10), HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };

    internal EmulationCoreManagementPanel(Func<string, object[], string> localize)
    {
        string L(string key, params object[] arguments) => localize(key, arguments);
        Search.Content = ButtonContent("\uE721", L("Emulation.Core.NameSearch"));
        Download.Content = ButtonContent("\uE896", L("Emulation.Core.NameDownload"));
        Cancel.Content = L("Common.Cancel");

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
            Text = ControlVisualConstants.GameControllerGlyph, FontFamily = ControlVisualConstants.IconFont,
            FontSize = 18, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0)
        });
        title.Children.Add(new TextBlock
        {
            Text = L("Emulation.Core.Emulator"), FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });
        installedRow.Children.Add(title);

        var installedPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        var installedLabel = new TextBlock
        {
            Text = L("Emulation.Core.NameProjectVersion"), FontSize = 11, Margin = new Thickness(0, 0, 0, 2)
        };
        installedLabel.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        installedPanel.Children.Add(installedLabel);
        Installed.VerticalAlignment = VerticalAlignment.Center;
        Installed.FontWeight = FontWeights.SemiBold;
        installedPanel.Children.Add(Installed);
        var installedBadge = new Border
        {
            Child = installedPanel, Padding = new Thickness(12, 6, 12, 6), CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1)
        };
        installedBadge.SetResourceReference(BackgroundProperty, "WindowBrush");
        installedBadge.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Grid.SetColumn(installedBadge, 1);
        installedRow.Children.Add(installedBadge);
        Search.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(Search, 2);
        installedRow.Children.Add(Search);
        panel.Children.Add(installedRow);

        var resultHost = new Grid { Margin = new Thickness(16, 4, 16, 2) };
        Prompt.VerticalAlignment = VerticalAlignment.Center;
        Prompt.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        var promptContent = new StackPanel { Orientation = Orientation.Horizontal };
        var promptIcon = new TextBlock
        {
            Text = ControlVisualConstants.InformationGlyph, FontFamily = ControlVisualConstants.IconFont,
            FontSize = 17, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 9, 0)
        };
        promptIcon.SetResourceReference(ForegroundProperty, "AccentBrush");
        promptContent.Children.Add(promptIcon);
        promptContent.Children.Add(Prompt);
        _promptBanner.Child = promptContent;
        _promptBanner.SetResourceReference(BackgroundProperty, "WindowBrush");
        _promptBanner.SetResourceReference(BorderBrushProperty, "BorderBrush");
        resultHost.Children.Add(_promptBanner);

        Results.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
        Results.RowDefinitions.Add(new RowDefinition());
        var availableVersions = new Grid();
        availableVersions.ColumnDefinitions.Add(new ColumnDefinition());
        availableVersions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        availableVersions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Versions.Margin = new Thickness(0, 0, 12, 0);
        availableVersions.Children.Add(Versions);
        Grid.SetColumn(Download, 1);
        availableVersions.Children.Add(Download);
        Cancel.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(Cancel, 2);
        availableVersions.Children.Add(Cancel);
        Results.Children.Add(availableVersions);

        var details = new Grid { Margin = new Thickness(0, 7, 0, 0) };
        details.ColumnDefinitions.Add(new ColumnDefinition());
        details.ColumnDefinitions.Add(new ColumnDefinition());
        details.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        details.Children.Add(DetailTile(L("Emulation.Core.NameRequiredVersion"), RequiredVersion,
            new Thickness(0, 0, 5, 0)));
        var latestTile = DetailTile(L("Emulation.Core.NameLatestVersion"), LatestVersion,
            new Thickness(5, 0, 5, 0));
        Grid.SetColumn(latestTile, 1);
        details.Children.Add(latestTile);
        FoundCount.VerticalAlignment = VerticalAlignment.Center;
        FoundCount.HorizontalAlignment = HorizontalAlignment.Center;
        FoundCount.FontWeight = FontWeights.SemiBold;
        var countTile = new Border
        {
            Child = FoundCount, CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(5, 0, 0, 0)
        };
        countTile.SetResourceReference(BackgroundProperty, "WindowBrush");
        countTile.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Grid.SetColumn(countTile, 2);
        details.Children.Add(countTile);
        Grid.SetRow(details, 1);
        Results.Children.Add(details);
        resultHost.Children.Add(Results);
        Grid.SetRow(resultHost, 1);
        panel.Children.Add(resultHost);

        var statusHost = new Grid { Margin = new Thickness(16, 0, 16, 6) };
        statusHost.RowDefinitions.Add(new RowDefinition());
        statusHost.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
        Status.VerticalAlignment = VerticalAlignment.Center;
        _statusBanner.Child = Status;
        statusHost.Children.Add(_statusBanner);
        Grid.SetRow(Progress, 1);
        statusHost.Children.Add(Progress);
        Grid.SetRow(statusHost, 2);
        panel.Children.Add(statusHost);

        var card = new Border { Child = panel, CornerRadius = new CornerRadius(9), BorderThickness = new Thickness(1) };
        card.SetResourceReference(BackgroundProperty, "CardBrush");
        card.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Content = card;
    }

    internal void ShowPrompt(string text)
    {
        Prompt.Text = text;
        Results.Visibility = Visibility.Hidden;
        _promptBanner.Visibility = Visibility.Visible;
    }

    internal void ShowResults()
    {
        Results.Visibility = Visibility.Visible;
        _promptBanner.Visibility = Visibility.Hidden;
        Versions.Visibility = Visibility.Visible;
        Download.Visibility = Visibility.Visible;
    }

    internal void SetStatus(string text, bool isError = false)
    {
        Status.Text = text;
        _statusBanner.Visibility = string.IsNullOrWhiteSpace(text) ? Visibility.Collapsed : Visibility.Visible;
        if (isError)
        {
            _statusBanner.Background = new SolidColorBrush(Color.FromRgb(255, 241, 241));
            _statusBanner.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 75, 75));
            Status.Foreground = new SolidColorBrush(Color.FromRgb(150, 25, 25));
            return;
        }
        _statusBanner.SetResourceReference(BackgroundProperty, "WindowBrush");
        _statusBanner.SetResourceReference(BorderBrushProperty, "BorderBrush");
        Status.SetResourceReference(ForegroundProperty, "TextBrush");
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
