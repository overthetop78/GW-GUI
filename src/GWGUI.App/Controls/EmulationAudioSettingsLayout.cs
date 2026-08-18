using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal static partial class EmulationSettingsLayout
{
    internal static ScrollViewer AudioSettingsPage(
        IReadOnlyList<FrameworkElement> outputFields,
        IReadOnlyList<FrameworkElement> qualityFields,
        IReadOnlyList<FrameworkElement>? driveFields = null,
        string? information = null)
    {
        var output = ActionCard(AudioFields(outputFields), LocExtension.Get("Emulation.Audio.Output"));
        var quality = ActionCard(AudioFields(qualityFields), LocExtension.Get("Emulation.Audio.Quality"));
        Grid page;
        var columnCount = 2;
        if (driveFields is null)
        {
            page = TwoColumnPage(output, quality);
        }
        else
        {
            page = ThreeColumnPage(output, quality,
                ActionCard(AudioFields(driveFields), LocExtension.Get("Emulation.Audio.Drives")));
            columnCount = 3;
        }

        if (!string.IsNullOrWhiteSpace(information))
        {
            var banner = InformationBanner(information);
            banner.Margin = new Thickness(0, 12, 0, 0);
            Grid.SetRow(banner, 1);
            Grid.SetColumnSpan(banner, columnCount);
            page.Children.Add(banner);
        }
        return ScrollPage(page);
    }

    internal static FrameworkElement AudioChoiceField(string label, FrameworkElement control)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 7), TextWrapping = TextWrapping.NoWrap });
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.Margin = new Thickness(0);
        panel.Children.Add(control);
        return panel;
    }

    internal static FrameworkElement AudioCheckBoxField(CheckBox checkBox)
    {
        checkBox.HorizontalAlignment = HorizontalAlignment.Left;
        checkBox.VerticalAlignment = VerticalAlignment.Center;
        checkBox.Margin = new Thickness(0, 6, 0, 6);
        return checkBox;
    }

    internal static FrameworkElement AudioPercentageField(string label, Slider slider)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 7), TextWrapping = TextWrapping.NoWrap });
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        slider.Margin = new Thickness(0, 0, 12, 0);
        slider.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(slider);
        var value = new TextBlock { MinWidth = 48, VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right };
        void RefreshValue() => value.Text = $"{slider.Value:0} %";
        slider.ValueChanged += (_, _) => RefreshValue();
        RefreshValue();
        Grid.SetColumn(value, 1);
        row.Children.Add(value);
        panel.Children.Add(row);
        return panel;
    }

    private static Grid AudioFields(IReadOnlyList<FrameworkElement> fields)
    {
        var grid = new Grid { Margin = new Thickness(14, 10, 14, 14) };
        for (var row = 0; row < fields.Count; row++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var row = 0; row < fields.Count; row++)
        {
            var field = fields[row];
            field.Margin = new Thickness(0, 4, 0, 10);
            Grid.SetRow(field, row);
            grid.Children.Add(field);
        }
        return grid;
    }
}
