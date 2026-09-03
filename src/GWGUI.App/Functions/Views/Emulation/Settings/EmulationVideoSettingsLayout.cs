using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Views.Emulation;
using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsLayout
{
    private const int VideoSettingsColumnCount = 2;

    internal static FrameworkElement VideoSettingsPage(FrameworkElement content) => content;

    internal static FrameworkElement VideoSettingsChoice(EmulationVideoSettingsField field)
    {
        var tile = field.IsTrailingCheckBox && field.Control is CheckBox checkBox
            ? TrailingCheckBoxTile(field.Label, checkBox)
            : LabeledSettingsTile(field.Label, field.Control);
        tile.Margin = new Thickness(0, 3, 0, 9);
        return tile;
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
