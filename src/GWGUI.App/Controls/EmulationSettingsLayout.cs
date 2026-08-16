using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Controls;

internal static class EmulationSettingsLayout
{
    internal static Grid TwoColumnPage(Border left, Border right)
    {
        var grid = new Grid { Margin = new Thickness(12) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        left.Margin = new Thickness(0, 0, 5, 0);
        right.Margin = new Thickness(5, 0, 0, 0);
        grid.Children.Add(left);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        return grid;
    }

    internal static Grid ThreeColumnPage(Border left, Border center, Border right)
    {
        var grid = new Grid { Margin = new Thickness(12) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        left.Margin = new Thickness(0, 0, 5, 0);
        center.Margin = new Thickness(5, 0, 5, 0);
        right.Margin = new Thickness(5, 0, 0, 0);
        grid.Children.Add(left);
        Grid.SetColumn(center, 1); grid.Children.Add(center);
        Grid.SetColumn(right, 2); grid.Children.Add(right);
        return grid;
    }

    internal static Grid CompactForm(int columns, params (string Label, FrameworkElement Control)[] fields)
    {
        var form = new Grid { Margin = new Thickness(10) };
        for (var column = 0; column < columns; column++)
        {
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 125 });
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 150 });
        }
        var rows = (int)Math.Ceiling(fields.Length / (double)columns);
        for (var row = 0; row < rows; row++) form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 0; index < fields.Length; index++)
        {
            var row = index / columns;
            var column = (index % columns) * 2;
            var label = new TextBlock { Text = fields[index].Label, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(column == 0 ? 0 : 18, 7, 10, 7), TextWrapping = TextWrapping.Wrap };
            Grid.SetRow(label, row); Grid.SetColumn(label, column); form.Children.Add(label);
            var control = fields[index].Control;
            control.Margin = new Thickness(0, 4, 0, 4);
            control.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(control, row); Grid.SetColumn(control, column + 1); form.Children.Add(control);
        }
        return form;
    }

    internal static Border InputBindings(InputBindingEditor editor, string title, string? hint = null)
    {
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        var heading = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 6, 10, 2) };
        heading.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center });
        if (!string.IsNullOrWhiteSpace(hint))
            heading.Children.Add(new TextBlock { Text = "\uE946", FontFamily = ControlVisualConstants.IconFont,
                FontSize = 15, Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center,
                ToolTip = hint });
        layout.Children.Add(heading);
        Grid.SetRow(editor, 1); layout.Children.Add(editor);
        var card = new Border { Child = layout, Padding = new Thickness(2) };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        return card;
    }

    internal static Border IconCard(UIElement child, string title, string icon) =>
        HeaderCard(child, title, new TextBlock { Text = icon, FontFamily = ControlVisualConstants.IconFont,
            FontSize = 19, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });

    internal static Border ActionCard(UIElement child, string title, FrameworkElement? actions = null)
    {
        var header = new Grid { Height = 54 };
        header.ColumnDefinitions.Add(new ColumnDefinition());
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 17,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 12, 0) });
        if (actions is not null) { Grid.SetColumn(actions, 1); actions.Margin = new Thickness(8, 8, 12, 8); header.Children.Add(actions); }
        return HeaderCard(child, header);
    }

    internal static Border InformationBanner(string text)
        => InformationBanner(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap });

    internal static Border InformationBanner(TextBlock text)
    {
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(12, 9, 12, 9)
        };
        var icon = new TextBlock
        {
            Text = "\uE946",
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        icon.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        content.Children.Add(icon);
        text.VerticalAlignment = VerticalAlignment.Center;
        text.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        content.Children.Add(text);
        var banner = new Border
        {
            Child = content,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Margin = new Thickness(12, 0, 12, 12)
        };
        banner.SetResourceReference(Border.BackgroundProperty, "WindowBrush");
        banner.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
        return banner;
    }

    internal static ScrollViewer ScrollPage(UIElement child)
    {
        var viewer = new ScrollViewer
        {
            Content = child, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, PanningMode = PanningMode.VerticalOnly
        };
        viewer.PreviewMouseWheel += (_, args) =>
        {
            if (FindNestedScrollViewer(args.OriginalSource as DependencyObject, viewer) is { ScrollableHeight: > 0 })
                return;
            if (viewer.ScrollableHeight <= 0) return;
            var offset = Math.Clamp(viewer.VerticalOffset - args.Delta, 0, viewer.ScrollableHeight);
            if (Math.Abs(offset - viewer.VerticalOffset) < 0.5) return;
            viewer.ScrollToVerticalOffset(offset);
            args.Handled = true;
        };
        return viewer;
    }

    private static ScrollViewer? FindNestedScrollViewer(DependencyObject? source, ScrollViewer page)
    {
        while (source is not null && !ReferenceEquals(source, page))
        {
            if (source is ScrollViewer nested) return nested;
            source = VisualTreeHelper.GetParent(source);
        }
        return null;
    }

    private static Border HeaderCard(UIElement child, string title, FrameworkElement icon)
    {
        var heading = new StackPanel { Orientation = Orientation.Horizontal };
        icon.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        heading.Children.Add(icon);
        heading.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = 17,
            VerticalAlignment = VerticalAlignment.Center });
        return HeaderCard(child, heading);
    }

    private static Border HeaderCard(UIElement child, UIElement header)
    {
        var headerBorder = new Border { Child = header, BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = header is Grid ? new Thickness(0) : new Thickness(16, 12, 16, 12) };
        headerBorder.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
        var body = new Border { Child = child, Padding = new Thickness(8) };
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        layout.Children.Add(headerBorder); Grid.SetRow(body, 1); layout.Children.Add(body);
        var card = new Border { Child = layout, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), ClipToBounds = true };
        ControlUiFactory.ApplyCardAppearance(card);
        return card;
    }
}
