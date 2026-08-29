using GWGUI.App.Constants.Emulation;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Views.Controls.Emulation.Options;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsLayout
{
    internal static Grid SettingsFields(int columns,
        params EmulationSettingsControlField[] fields) => SettingsFieldGrid(columns, fields);

    internal static Grid SettingsFields(int columns,
        params (string Label, FrameworkElement Control)[] fields) => SettingsFieldGrid(columns,
        fields.Select(field => new EmulationSettingsControlField(field.Label, field.Control)).ToArray());

    private static Grid SettingsFieldGrid(params EmulationSettingsControlField[] fields) =>
        SettingsFieldGrid(1, fields);

    private static Grid SettingsFieldGrid(int columns, params EmulationSettingsControlField[] fields)
    {
        var grid = new Grid { Margin = new Thickness(12, 6, 12, 10) };
        for (var column = 0; column < columns; column++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(EmulationHardwareSettingsConstants.FieldLabelWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        var rowCount = (int)Math.Ceiling(fields.Length / (double)columns);
        for (var row = 0; row < rowCount; row++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 0; index < fields.Length; index++)
        {
            var row = index / columns;
            var column = index % columns * 2;
            var field = fields[index];
            var label = new EmulationSettingsFieldLabel(field.Label, field.Explanation,
                field.DetailedExplanation, field.Control)
            {
                Margin = new Thickness(column == 0 ? 0 : 18, 8, 10, 8)
            };
            label.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(UIElement.Visibility))
            {
                Source = field.Control,
                Mode = BindingMode.OneWay
            });
            Grid.SetRow(label, row);
            Grid.SetColumn(label, column);
            grid.Children.Add(label);
            var control = field.Control;
            control.MinWidth = control is CheckBox
                ? 0
                : EmulationHardwareSettingsConstants.FieldControlMinimumWidth;
            control.Margin = new Thickness(0, 4, 0, 4);
            control.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(control, row);
            Grid.SetColumn(control, column + 1);
            grid.Children.Add(control);
        }
        return grid;
    }
}
