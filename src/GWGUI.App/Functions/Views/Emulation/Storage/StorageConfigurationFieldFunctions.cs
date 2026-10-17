using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Functions.Views.Emulation.Storage;

internal static class StorageConfigurationFieldFunctions
{
    internal static string Read(FrameworkElement control) => control switch
    {
        Panel { Tag: Func<string> read } => read(),
        CheckBox { Tag: EmulationSettingsField field } toggle => toggle.IsChecked == true ? field.EnabledValue : field.DisabledValue,
        ComboBox { SelectedItem: EmulationSettingsChoiceView choice } => choice.Choice.Id,
        Slider slider => slider.Value.ToString(CultureInfo.InvariantCulture),
        _ => string.Empty
    };

    internal static FrameworkElement Create(EmulationSettingsField field, string? moduleId, IReadOnlyDictionary<string, string>? values)
    {
        var value = values?.GetValueOrDefault(field.Id) ?? field.Value;
        FrameworkElement control;
        if (field.Editor == EmulationSettingsEditor.Toggle)
            control = new CheckBox { IsChecked = value == field.EnabledValue, Tag = field };
        else if (field.NumericRange is { } range)
        {
            var number = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : range.Minimum;
            control = new Slider { Minimum = range.Minimum, Maximum = range.Maximum, TickFrequency = range.Step,
                SmallChange = range.Step, LargeChange = range.Step, IsSnapToTickEnabled = true, Value = Math.Clamp(number, range.Minimum, range.Maximum) };
        }
        else
        {
            var choices = (field.Choices ?? []).Select(choice => new EmulationSettingsChoiceView(choice,
                EmulationSettingsValuePresentationFunctions.WithUnit(choice.InvariantDisplayValue ?? LocExtension.GetForModule(moduleId ?? string.Empty, choice.DisplayResourceKey), field.ValueUnitResourceKey))).ToArray();
            if (choices.Length == 2)
            {
                var radios = new WrapPanel();
                var group = Guid.NewGuid().ToString();
                var selected = choices.FirstOrDefault(choice => choice.Choice.Id == value) ?? choices[0];
                foreach (var choice in choices)
                    radios.Children.Add(new RadioButton { Content = new TextBlock { Text = choice.DisplayName, TextWrapping = TextWrapping.Wrap }, Tag = choice,
                        GroupName = group, IsChecked = choice.Choice.Id == selected.Choice.Id, Margin = new Thickness(0, 4, 14, 4) });
                radios.Tag = (Func<string>)(() => radios.Children.OfType<RadioButton>()
                    .Where(radio => radio.IsChecked == true).Select(radio => ((EmulationSettingsChoiceView)radio.Tag).Choice.Id).FirstOrDefault() ?? value ?? string.Empty);
                control = radios;
            }
            else
            {
                var selection = new ComboBox { ItemsSource = choices, SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == value) ?? choices.FirstOrDefault() };
                var text = new FrameworkElementFactory(typeof(TextBlock));
                text.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(EmulationSettingsChoiceView.DisplayName)));
                text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                selection.ItemTemplate = new DataTemplate { VisualTree = text };
                control = selection;
            }
        }
        control.IsEnabled = field.IsEnabled;
        return control;
    }

    internal static FrameworkElement Present(EmulationSettingsField field, FrameworkElement control, string? moduleId)
    {
        FrameworkElement presentation = control;
        if (control is Slider slider)
        {
            var number = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
            void UpdateNumber()
            {
                var choice = field.UseChoiceLabelsForSlider ? field.Choices?.FirstOrDefault(item => item.Id == slider.Value.ToString(CultureInfo.InvariantCulture)) : null;
                number.Text = choice is not null ? EmulationSettingsValuePresentationFunctions.WithUnit(choice.InvariantDisplayValue ?? LocExtension.GetForModule(moduleId ?? string.Empty, choice.DisplayResourceKey), field.ValueUnitResourceKey)
                    : EmulationSettingsValuePresentationFunctions.WithUnit(slider.Value.ToString(CultureInfo.InvariantCulture), field.ValueUnitResourceKey) + field.NumericRange!.UnitSuffix;
            }
            UpdateNumber();
            slider.ValueChanged += (_, _) => UpdateNumber();
            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(slider);
            Grid.SetColumn(number, 1); row.Children.Add(number);
            presentation = row;
        }
        return EmulationSettingsLayout.VideoSettingsChoice(new GWGUI.App.Contracts.Views.Emulation.Settings.EmulationVideoSettingsField(
            LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey), presentation,
            IsTrailingCheckBox: control is CheckBox,
            Explanation: field.ExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.ExplanationResourceKey),
            DetailedExplanation: field.DetailedExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.DetailedExplanationResourceKey)));
    }
}
