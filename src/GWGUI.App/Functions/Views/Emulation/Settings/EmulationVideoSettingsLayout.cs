using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Views.Emulation;
using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Emulation.Options;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsLayout
{
    private const int VideoSettingsColumnCount = 2;

    internal static FrameworkElement VideoSettingsPage(FrameworkElement content) => content;

    internal static FrameworkElement VideoSettingsChoice(EmulationVideoSettingsField field)
    {
        var tile = field.IsTrailingCheckBox && field.Control is CheckBox checkBox ? TrailingCheckBoxTile(field, checkBox) : LabeledSettingsTile(field);
        tile.Margin = new Thickness(0, 3, 0, 9);
        return tile;
    }

    internal static Grid VideoSettingsFields(params EmulationVideoSettingsField[] fields) => VideoSettingsFields(VideoSettingsColumnCount, fields);

    internal static Grid VideoSettingsFields(int columns, params EmulationVideoSettingsField[] fields)
    {
        var grid = new Grid { Margin = new Thickness(14, 10, 14, 14) };
        for (var column = 0; column < columns; column++) grid.ColumnDefinitions.Add(new ColumnDefinition());
        var occupiedCells = fields.Sum(field => field.ColumnSpan);
        var rowCount = (int)Math.Ceiling(occupiedCells / (double)columns);
        for (var row = 0; row < rowCount; row++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var cell = 0;
        foreach (var field in fields)
        {
            var tile = field.IsTrailingCheckBox && field.Control is CheckBox checkBox ? TrailingCheckBoxTile(field, checkBox) : LabeledSettingsTile(field);
            var column = cell % columns;
            tile.Margin = new Thickness(column == 0 ? 0 : 12, 4, column + field.ColumnSpan >= columns ? 0 : 12, 10);
            Grid.SetRow(tile, cell / columns);
            Grid.SetColumn(tile, column);
            Grid.SetColumnSpan(tile, field.ColumnSpan);
            grid.Children.Add(tile);
            cell += field.ColumnSpan;
        }
        return grid;
    }

    internal static Grid VideoSettingsCardRow(FrameworkElement left, FrameworkElement right)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 18) };
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        row.Children.Add(left);
        row.Children.Add(right);
        left.VerticalAlignment = right.VerticalAlignment = VerticalAlignment.Top;
        void Arrange()
        {
            var sideBySide = row.ActualWidth >= 760;
            row.ColumnDefinitions[1].Width = sideBySide ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            Grid.SetColumn(right, sideBySide ? 1 : 0);
            Grid.SetRow(right, sideBySide ? 0 : 1);
            left.Margin = new Thickness(0, 0, sideBySide ? 9 : 0, sideBySide ? 0 : 18);
            right.Margin = new Thickness(sideBySide ? 9 : 0, 0, 0, 0);
        }
        row.SizeChanged += (_, _) => Arrange();
        Arrange();
        return row;
    }

    private static FrameworkElement LabeledSettingsTile(EmulationVideoSettingsField field)
    {
        var panel = new StackPanel();
        panel.Children.Add(new EmulationSettingsFieldLabel(field.Label, field.Explanation,
            field.DetailedExplanation, field.Control)
        {
            Margin = new Thickness(0, 0, 0, 7),
            TextWrapping = TextWrapping.Wrap
        });
        field.Control.HorizontalAlignment = HorizontalAlignment.Stretch;
        field.Control.Margin = new Thickness(0);
        panel.Children.Add(field.Control);
        return panel;
    }

    private static FrameworkElement TrailingCheckBoxTile(EmulationVideoSettingsField field, CheckBox checkBox)
    {
        var row = new DockPanel { VerticalAlignment = VerticalAlignment.Center, LastChildFill = true };
        checkBox.Content = null;
        checkBox.Margin = new Thickness(0, 4, 10, 4);
        checkBox.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(checkBox);
        row.Children.Add(new EmulationSettingsFieldLabel(field.Label, field.Explanation,
            field.DetailedExplanation, checkBox) { TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 0, 4) });
        return row;
    }
}
