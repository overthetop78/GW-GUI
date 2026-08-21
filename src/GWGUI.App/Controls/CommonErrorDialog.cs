using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using GWGUI.App.Constants;

namespace GWGUI.App.Controls;

internal sealed class CommonErrorDialog : Window
{
    internal const string ErrorIcon = "\uEA39";

    internal CommonErrorDialog(CommonErrorDialogContent content)
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        MinWidth = 480;
        MaxWidth = 680;

        var heading = new TextBlock
        {
            Text = content.Heading,
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        };
        var message = new TextBlock
        {
            Text = content.Message,
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 520
        };
        var text = new StackPanel();
        text.Children.Add(heading);
        text.Children.Add(message);
        if (content.Details is { Count: > 0 }) text.Children.Add(Details(content.Details));

        var body = new Grid { Margin = new Thickness(26, 24, 26, 18) };
        body.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = content.MediaIcons is { Count: > 0 } ? new GridLength(104) : GridLength.Auto
        });
        body.ColumnDefinitions.Add(new ColumnDefinition());
        body.Children.Add(content.MediaIcons is { Count: > 0 }
            ? MediaIconPanel(content.MediaIcons)
            : SingleIcon(content.Icon, content.IconBrush));
        Grid.SetColumn(text, 1);
        body.Children.Add(text);

        var ok = new Button
        {
            Content = "OK",
            IsDefault = true,
            IsCancel = true,
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 22, 18)
        };
        ok.Click += (_, _) => DialogResult = true;
        AutomationProperties.SetName(ok, "OK");

        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition());
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.Children.Add(body);
        Grid.SetRow(ok, 1);
        layout.Children.Add(ok);

        var card = new Border
        {
            Child = layout,
            Background = (Brush)Application.Current.FindResource("CardBrush"),
            BorderBrush = (Brush)Application.Current.FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Effect = new DropShadowEffect { BlurRadius = 24, ShadowDepth = 5, Opacity = 0.28 }
        };
        Content = card;
    }

    internal static void Show(FrameworkElement owner, CommonErrorDialogContent content)
    {
        var dialog = new CommonErrorDialog(content) { Owner = Window.GetWindow(owner) };
        dialog.ShowDialog();
    }

    private static FrameworkElement Details(IEnumerable<CommonErrorDialogDetail> details)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 16, 0, 0) };
        foreach (var detail in details)
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.Children.Add(new TextBlock
            {
                Text = detail.Label,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 10, 0)
            });
            var value = new TextBlock { Text = detail.Value, TextWrapping = TextWrapping.Wrap };
            Grid.SetColumn(value, 1);
            row.Children.Add(value);
            panel.Children.Add(row);
        }
        return panel;
    }

    private static FrameworkElement SingleIcon(string glyph, Brush brush) => new TextBlock
    {
        Text = glyph,
        FontFamily = ControlVisualConstants.IconFont,
        FontSize = 34,
        Foreground = brush,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new Thickness(0, 1, 18, 0)
    };

    private static FrameworkElement MediaIconPanel(IReadOnlyList<CommonErrorDialogMediaIcon> icons)
    {
        var panel = new Grid
        {
            Height = 176,
            Margin = new Thickness(2, 22, 22, 22),
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (var _ in icons) panel.RowDefinitions.Add(new RowDefinition());
        for (var index = 0; index < icons.Count; index++)
        {
            var icon = new Viewbox
            {
                Child = new TextBlock
                {
                    Text = MediaGlyph(icons[index]),
                    FontFamily = ControlVisualConstants.IconFont
                },
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 96,
                MaxHeight = 160d / icons.Count,
                Margin = new Thickness(4)
            };
            ((TextBlock)icon.Child).SetResourceReference(TextBlock.ForegroundProperty, "TextBrush");
            Grid.SetRow(icon, index);
            panel.Children.Add(icon);
        }
        return panel;
    }

    private static string MediaGlyph(CommonErrorDialogMediaIcon icon) => icon switch
    {
        CommonErrorDialogMediaIcon.Floppy => MachinePresentationConstants.FloppyGlyph,
        CommonErrorDialogMediaIcon.HardDisk => MachinePresentationConstants.HardDiskGlyph,
        CommonErrorDialogMediaIcon.CompactDisc => MachinePresentationConstants.CompactDiscGlyph,
        CommonErrorDialogMediaIcon.Cartridge => MachinePresentationConstants.CartridgeGlyph,
        CommonErrorDialogMediaIcon.Cassette => MachinePresentationConstants.CassetteGlyph,
        _ => string.Empty
    };
}
