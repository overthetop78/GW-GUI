using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Factories.Views.Common;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;


namespace GWGUI.App.Functions.Views.Emulation.Storage;

internal static class StorageDialogUi
{
    /// <summary>Conserve les libellés détaillés lisibles dans la sélection et dans la liste déroulante.</summary>
    internal static void WrapChoices(ComboBox choices, string property)
    {
        var text = new FrameworkElementFactory(typeof(TextBlock));
        text.SetBinding(TextBlock.TextProperty, new Binding(property));
        text.SetBinding(FrameworkElement.ToolTipProperty, new Binding(property));
        text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        text.SetBinding(FrameworkElement.MaxWidthProperty, new Binding(nameof(FrameworkElement.ActualWidth))
        {
            Source = choices, Converter = new ChoiceWidthConverter()
        });
        choices.DisplayMemberPath = string.Empty;
        choices.Padding = new Thickness(10, 3, 30, 3);
        choices.ItemTemplate = new DataTemplate { VisualTree = text };
    }

    private sealed class ChoiceWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double width && width > 0 ? Math.Max(1, width - 44) : 520d;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public static Grid DialogLayout(params UIElement[] elements)
    {
        var root = new Grid { Margin = new Thickness(18) };
        for (var index = 0; index < elements.Length; index++)
            root.RowDefinitions.Add(new RowDefinition { Height = index == elements.Length - 1 ? GridLength.Auto : new GridLength(1, GridUnitType.Auto) });
        for (var index = 0; index < elements.Length; index++)
        {
            Grid.SetRow(elements[index], index);
            root.Children.Add(elements[index]);
        }
        return root;
    }

    public static FrameworkElement DialogHeader(string icon, string title, string subtitle, FrameworkElement? details = null, bool compact = false)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 0, 4, 12) };
        panel.Children.Add(new TextBlock
        {
            Text = icon,
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = compact ? 22 : 28,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0)
        });
        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, FontSize = compact ? 16 : 20, TextWrapping = TextWrapping.Wrap });
        text.Children.Add(new TextBlock { Text = subtitle, Margin = new Thickness(0, 3, 0, 0) });
        if (details is not null) text.Children.Add(details);
        panel.Children.Add(text);
        return panel;
    }

    public static Border Card(string title, UIElement content, bool compact = false)
    {
        var panel = new StackPanel();
        if (!string.IsNullOrWhiteSpace(title)) panel.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = compact ? 14 : 16,
            Margin = new Thickness(0, 0, 0, 12)
        });
        panel.Children.Add(content);
        var card = new Border { Child = panel, Margin = new Thickness(0, 0, 0, 12) };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        if (compact) card.Padding = new Thickness(10);
        return card;
    }

    public static Border IconCard(string icon, string title, UIElement content, bool compact = false)
    {
        var header = new StackPanel { Orientation = Orientation.Horizontal };
        var glyph = new TextBlock
        {
            Text = icon,
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = compact ? 16 : 18,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 9, 0)
        };
        glyph.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        header.Children.Add(glyph);
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = compact ? 14 : 16,
            VerticalAlignment = VerticalAlignment.Center
        });
        var panel = new StackPanel();
        panel.Children.Add(header);
        var separator = new Border { Height = 1, Margin = new Thickness(0, 10, 0, 8) };
        separator.SetResourceReference(Border.BackgroundProperty, "BorderBrush");
        panel.Children.Add(separator);
        panel.Children.Add(content);
        var card = new Border
        {
            Child = panel,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(compact ? 10 : 14),
            Margin = new Thickness(0, 0, 0, 10)
        };
        ControlUiFactory.ApplyCardAppearance(card);
        return card;
    }

    public static void CompactAppearance(Window window)
    {
        window.FontSize = 13;
        window.FlowDirection = System.Globalization.CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        foreach (var type in new[] { typeof(Button), typeof(ComboBox) })
        {
            var style = new Style(type, window.TryFindResource(type) as Style);
            style.Setters.Add(new Setter(Control.MinHeightProperty, 28d));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10, 3, 10, 3)));
            window.Resources[type] = style;
        }
    }

    public static Grid CompactColumns(FrameworkElement left, FrameworkElement right)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        left.VerticalAlignment = right.VerticalAlignment = VerticalAlignment.Top;
        grid.Children.Add(left); grid.Children.Add(right);
        void Arrange()
        {
            var wide = grid.ActualWidth >= 620;
            grid.ColumnDefinitions[1].Width = wide ? new GridLength(210) : new GridLength(0);
            Grid.SetColumn(right, wide ? 1 : 0); Grid.SetRow(right, wide ? 0 : 1);
            left.Margin = new Thickness(0, 0, wide ? 10 : 0, 10);
            right.Margin = new Thickness(0, 0, 0, 10);
        }
        grid.SizeChanged += (_, _) => Arrange();
        Arrange();
        return grid;
    }

    public static Grid SideBySide(FrameworkElement left, FrameworkElement right)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        left.Margin = new Thickness(0, 0, 5, 10);
        right.Margin = new Thickness(5, 0, 0, 10);
        grid.Children.Add(left);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        return grid;
    }

    public static Grid CompactFields(params (string Label, FrameworkElement Control)[] fields)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var index = 0; index < fields.Length; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new TextBlock
            {
                Text = fields[index].Label,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 10, 6)
            };
            Grid.SetRow(label, index);
            grid.Children.Add(label);
            var control = fields[index].Control;
            control.Margin = new Thickness(0, 4, 0, 4);
            control.MaxWidth = 310;
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(control, index);
            Grid.SetColumn(control, 1);
            grid.Children.Add(control);
        }
        return grid;
    }

    public static Grid TwoColumnFields(params (string Label, FrameworkElement Control)[] fields)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        var rowCount = (int)Math.Ceiling(fields.Length / 2d);
        for (var row = 0; row < rowCount; row++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 0; index < fields.Length; index++)
        {
            var row = index / 2;
            var column = (index % 2) * 2;
            var label = new TextBlock
            {
                Text = fields[index].Label,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(column == 0 ? 0 : 18, 6, 8, 6)
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, column);
            grid.Children.Add(label);
            fields[index].Control.Margin = new Thickness(0, 4, 0, 4);
            Grid.SetRow(fields[index].Control, row);
            Grid.SetColumn(fields[index].Control, column + 1);
            grid.Children.Add(fields[index].Control);
        }
        return grid;
    }

    public static Grid Field(string label, FrameworkElement control)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
        Grid.SetColumn(control, 1);
        grid.Children.Add(control);
        return grid;
    }

    public static Grid PathField(string label, TextBox textBox, Action? browse)
    {
        var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(string.IsNullOrWhiteSpace(label) ? 0 : 130) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        if (browse is not null) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        Grid.SetColumn(textBox, 1);
        grid.Children.Add(textBox);
        if (browse is not null)
        {
            var button = ControlUiFactory.TextButton(LocExtension.Get("Common.Browse"), 110, (_, _) => browse());
            Grid.SetColumn(button, 2);
            grid.Children.Add(button);
        }
        return grid;
    }

    public static Border Info(string text)
    {
        var block = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        block.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        var border = new Border
        {
            Child = block,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 12, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(244, 248, 255))
        };
        border.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
        return border;
    }

    public static StackPanel Footer(Window window, string acceptText, Action? accept = null, bool compact = false)
    {
        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 2, 0, 0)
        };
        var cancel = new Button { Content = LocExtension.Get("Common.Cancel"), IsCancel = true, MinWidth = compact ? 90 : 110 };
        var ok = new Button { Content = acceptText, IsDefault = true, MinWidth = compact ? 100 : 140 };
        ok.Click += (_, _) =>
        {
            if (accept is null) window.DialogResult = true;
            else accept();
        };
        footer.Children.Add(cancel);
        footer.Children.Add(ok);
        return footer;
    }
}
