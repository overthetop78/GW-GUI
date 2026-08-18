using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal sealed record EmulationVideoSettingsField(
    string Label,
    FrameworkElement Control,
    int ColumnSpan = 1,
    bool IsTrailingCheckBox = false);

internal static partial class EmulationSettingsLayout
{
    private const int VideoSettingsColumnCount = 2;

    internal static ScrollViewer VideoSettingsPage(UIElement displaySettings, UIElement renderingSettings,
        Border? additionalSettings = null)
    {
        var page = TwoColumnPage(
            ActionCard(displaySettings, LocExtension.Get("Emulation.Video.Settings.Display")),
            ActionCard(renderingSettings, LocExtension.Get("Emulation.Video.Settings.Rendering")));
        if (additionalSettings is not null)
        {
            additionalSettings.Margin = new Thickness(0, 10, 0, 0);
            Grid.SetRow(additionalSettings, 1);
            Grid.SetColumnSpan(additionalSettings, 2);
            page.Children.Add(additionalSettings);
        }
        return ScrollPage(page);
    }

    internal static Grid VideoSettingsFields(params EmulationVideoSettingsField[] fields)
    {
        var grid = new Grid { Margin = new Thickness(14, 10, 14, 14) };
        for (var column = 0; column < VideoSettingsColumnCount; column++) grid.ColumnDefinitions.Add(new ColumnDefinition());
        var occupiedCells = fields.Sum(field => field.ColumnSpan);
        var rowCount = (int)Math.Ceiling(occupiedCells / (double)VideoSettingsColumnCount);
        for (var row = 0; row < rowCount; row++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var cell = 0;
        foreach (var field in fields)
        {
            var tile = field.IsTrailingCheckBox && field.Control is CheckBox checkBox
                ? TrailingCheckBoxTile(field.Label, checkBox)
                : LabeledSettingsTile(field.Label, field.Control);
            var column = cell % VideoSettingsColumnCount;
            tile.Margin = new Thickness(column == 0 ? 0 : 12, 4,
                column + field.ColumnSpan >= VideoSettingsColumnCount ? 0 : 12, 10);
            Grid.SetRow(tile, cell / VideoSettingsColumnCount);
            Grid.SetColumn(tile, column);
            Grid.SetColumnSpan(tile, field.ColumnSpan);
            grid.Children.Add(tile);
            cell += field.ColumnSpan;
        }
        return grid;
    }

    private static FrameworkElement LabeledSettingsTile(string label, FrameworkElement control)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 7), TextWrapping = TextWrapping.NoWrap });
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.Margin = new Thickness(0);
        panel.Children.Add(control);
        return panel;
    }

    private static FrameworkElement TrailingCheckBoxTile(string label, CheckBox checkBox)
    {
        var row = new Grid { VerticalAlignment = VerticalAlignment.Bottom, MinHeight = 65 };
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.NoWrap,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 22, 12, 0) });
        checkBox.Content = null;
        checkBox.Margin = new Thickness(8, 22, 0, 0);
        checkBox.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(checkBox, 1);
        row.Children.Add(checkBox);
        return row;
    }
}
