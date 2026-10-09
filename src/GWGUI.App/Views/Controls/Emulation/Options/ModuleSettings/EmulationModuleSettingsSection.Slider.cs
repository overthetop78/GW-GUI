using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GWGUI.App.Constants.Views.Emulation;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection
{
    private FrameworkElement CreateSlider(EmulationSettingsField field)
    {
        var range = field.NumericRange ?? throw new ArgumentException(nameof(field));
        var value = double.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture,
            out var parsed) ? parsed : range.Minimum;
        var slider = new Slider
        {
            Minimum = range.Minimum,
            Maximum = range.Maximum,
            TickFrequency = range.Step,
            SmallChange = range.Step,
            LargeChange = range.Step,
            IsSnapToTickEnabled = true,
            Value = Math.Clamp(value, range.Minimum, range.Maximum),
            VerticalAlignment = VerticalAlignment.Center,
        };
        var label = new TextBlock
        {
            MinWidth = EmulationSliderConstants.ValueLabelMinimumWidth,
            Margin = new Thickness(EmulationSliderConstants.ValueLabelSpacing, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
        };
        UpdateLabel();
        var savedValue = slider.Value;
        slider.ValueChanged += (_, _) => UpdateLabel();
        slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(
            async (_, _) => await SaveValueAsync()));
        slider.PreviewMouseLeftButtonUp += async (_, _) =>
        {
            if (!slider.IsMouseCaptureWithin) await SaveValueAsync();
        };
        slider.PreviewKeyUp += async (_, _) => await SaveValueAsync();
        var row = new Grid { Tag = slider };
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(slider);
        Grid.SetColumn(label, 1);
        row.Children.Add(label);
        return row;

        void UpdateLabel() => label.Text = slider.Value.ToString(
            EmulationSliderConstants.NumericFormat, CultureInfo.CurrentCulture) + range.UnitSuffix;

        async Task SaveValueAsync()
        {
            if (slider.Value == savedValue) return;
            savedValue = slider.Value;
            await ExecuteUserChangeAsync();
        }
    }
}
