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
        var value = double.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : range.Minimum;
        var slider = new Slider
        {
            Minimum = range.Minimum,
            Maximum = range.Maximum,
            TickFrequency = range.Step,
            SmallChange = range.Step,
            LargeChange = range.Step,
            IsSnapToTickEnabled = true,
            TickPlacement = range.Step > 1 && (range.Maximum - range.Minimum) / range.Step <= 32 ? TickPlacement.BottomRight : TickPlacement.None,
            Orientation = range.IsVertical ? Orientation.Vertical : Orientation.Horizontal,
            IsDirectionReversed = range.IsVertical,
            Height = range.IsVertical ? 180 : double.NaN,
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
        slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(async (_, _) => await SaveValueAsync()));
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
        if (field.UseChoiceLabelsForSlider)
        {
            row.ColumnDefinitions.Clear();
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(label, 0);
            Grid.SetRow(label, 1);
            label.Margin = new Thickness(0, 6, 0, 0);
            label.TextAlignment = TextAlignment.Left;
            label.TextWrapping = TextWrapping.Wrap;
        }
        if (field.AutomaticValue is { } automaticValue)
        {
            var automatic = new CheckBox
            {
                Content = GWGUI.App.Localization.Extensions.LocExtension.Get("Visual.Automatic"),
                IsChecked = field.Value == automaticValue, Margin = new Thickness(0, 0, 0, 8)
            };
            slider.IsEnabled = automatic.IsChecked != true;
            var panel = new StackPanel();
            panel.Children.Add(automatic);
            panel.Children.Add(row);
            panel.Tag = (Func<string>)(() => automatic.IsChecked == true ? automaticValue
                : slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.InvariantCulture));
            automatic.Checked += async (_, _) => { slider.IsEnabled = false; await ExecuteUserChangeAsync(); };
            automatic.Unchecked += async (_, _) => { slider.IsEnabled = true; await ExecuteUserChangeAsync(); };
            return panel;
        }
        return row;

        void UpdateLabel()
        {
            var choice = field.UseChoiceLabelsForSlider ? field.Choices?.FirstOrDefault(item => item.Id == slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.InvariantCulture)) : null;
            label.Text = choice is not null ? ChoiceView(choice).DisplayName
                : slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.CurrentCulture) + range.UnitSuffix;
        }

        async Task SaveValueAsync()
        {
            if (slider.Value == savedValue) return;
            savedValue = slider.Value;
            await ExecuteUserChangeAsync();
        }
    }
}
