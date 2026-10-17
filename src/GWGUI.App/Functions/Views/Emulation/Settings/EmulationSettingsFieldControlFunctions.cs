using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GWGUI.App.Constants.Views.Emulation;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsFieldControlFunctions
{
    internal static string? Read(FrameworkElement control) => control switch
    {
        Panel { Tag: Func<string> read } => read(),
        ComboBox { SelectedItem: EmulationSettingsChoiceView choice } => choice.Choice.Id,
        CheckBox { Tag: EmulationSettingsField field } toggle => toggle.IsChecked == true ? field.EnabledValue : field.DisabledValue,
        Grid { Tag: Slider slider } => slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.InvariantCulture),
        Slider slider => slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.InvariantCulture),
        _ => null
    };

    internal static CheckBox CreateToggle(EmulationSettingsField field) => new() { IsChecked = field.Value == field.EnabledValue, Tag = field };

    internal static FrameworkElement CreateSelection(EmulationSettingsField field, IReadOnlyList<EmulationSettingsChoiceView> choices)
    {
        if (choices.Count == 2 && field.ChoiceSource == EmulationSettingsChoiceSource.Declared)
        {
            var radios = new WrapPanel();
            var group = Guid.NewGuid().ToString();
            var selected = choices.FirstOrDefault(choice => choice.Choice.Id == field.Value) ?? choices[0];
            foreach (var choice in choices)
            {
                var radio = new RadioButton { Content = new TextBlock { Text = choice.DisplayName, TextWrapping = TextWrapping.Wrap }, Tag = choice,
                    GroupName = group, IsChecked = choice.Choice.Id == selected.Choice.Id,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 4, 18, 4) };
                System.Windows.Automation.AutomationProperties.SetName(radio, choice.DisplayName);
                radios.Children.Add(radio);
            }
            radios.Tag = (Func<string>)(() => radios.Children.OfType<RadioButton>()
                .Where(radio => radio.IsChecked == true).Select(radio => ((EmulationSettingsChoiceView)radio.Tag).Choice.Id).FirstOrDefault() ?? field.Value ?? string.Empty);
            return radios;
        }
        var selection = new ComboBox { ItemsSource = choices,
            SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == field.Value) ?? choices.FirstOrDefault() };
        var text = new FrameworkElementFactory(typeof(TextBlock));
        text.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(EmulationSettingsChoiceView.DisplayName)));
        text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        selection.ItemTemplate = new DataTemplate { VisualTree = text };
        return selection;
    }

    internal static FrameworkElement CreateSlider(EmulationSettingsField field, Func<EmulationSettingsChoice, string> choiceLabel, Func<Task>? changed = null)
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
            Height = range.IsVertical ? 128 : double.NaN,
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
        if (range.IsVertical || field.UseChoiceLabelsForSlider)
        {
            row.ColumnDefinitions.Clear();
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(label, 0); Grid.SetRow(label, 1);
            label.Margin = new Thickness(0, 6, 0, 0);
            label.TextAlignment = TextAlignment.Left;
            label.TextWrapping = TextWrapping.Wrap;
            if (range.IsVertical)
            {
                slider.HorizontalAlignment = HorizontalAlignment.Center; slider.Width = 30;
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.TextAlignment = TextAlignment.Center; label.MinWidth = 0;
            }
        }
        if (field.AutomaticValue is { } automaticValue)
        {
            var automatic = new CheckBox { Content = LocExtension.Get("Visual.Automatic"),
                IsChecked = field.Value == automaticValue, Margin = new Thickness(0, 0, 0, 8) };
            slider.IsEnabled = automatic.IsChecked != true;
            var panel = new StackPanel();
            panel.Children.Add(automatic); panel.Children.Add(row);
            panel.Tag = (Func<string>)(() => automatic.IsChecked == true ? automaticValue
                : slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.InvariantCulture));
            automatic.Checked += async (_, _) => { slider.IsEnabled = false; if (changed is not null) await changed(); };
            automatic.Unchecked += async (_, _) => { slider.IsEnabled = true; if (changed is not null) await changed(); };
            return panel;
        }
        return row;

        void UpdateLabel()
        {
            var choice = field.UseChoiceLabelsForSlider ? field.Choices?.FirstOrDefault(item => item.Id == slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.InvariantCulture)) : null;
            label.Text = choice is not null ? choiceLabel(choice)
                : EmulationSettingsValuePresentationFunctions.WithUnit(slider.Value.ToString(EmulationSliderConstants.NumericFormat, CultureInfo.InvariantCulture), field.ValueUnitResourceKey) + range.UnitSuffix;
        }

        async Task SaveValueAsync()
        {
            if (slider.Value == savedValue) return;
            savedValue = slider.Value;
            if (changed is not null) await changed();
        }
    }
}
