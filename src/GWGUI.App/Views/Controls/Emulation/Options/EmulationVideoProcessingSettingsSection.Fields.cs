using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Views.Emulation;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationVideoProcessingSettingsSection : UserControl
{
    private static StackPanel Section(string resourceKey)
    {
        var section = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        section.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(resourceKey),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        return section;
    }

    private FrameworkElement ChoiceField<T>(string labelResourceKey,
        IReadOnlyDictionary<T, string> resourceKeys, T selected, Action<T> changed,
        string? automationId = null) where T : struct, Enum
    {
        var choices = resourceKeys.Select(choice => new Choice<T>(choice.Key,
            LocExtension.Get(choice.Value))).ToArray();
        var selector = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(Choice<T>.DisplayName),
            SelectedItem = choices.First(choice => EqualityComparer<T>.Default.Equals(choice.Value, selected)),
            MinWidth = 180
        };
        AutomationProperties.SetAutomationId(selector, automationId ?? typeof(T).Name);
        selector.SelectionChanged += (_, _) =>
        {
            if (!_loading && selector.SelectedItem is Choice<T> choice) changed(choice.Value);
        };
        return Field(labelResourceKey, selector);
    }

    private void AddIntensity(Panel panel, string id, int value, Action<int> changed) =>
        AddSlider(panel, id, value, EmulationVideoProcessingLimits.IntensityMinimum,
            EmulationVideoProcessingLimits.IntensityMaximum, changed);

    private void AddOptionalIntensity(Panel panel, string id, int? value, Action<int?> changed)
    {
        var enabled = value.HasValue;
        var row = new StackPanel();
        var toggle = new CheckBox { IsChecked = enabled };
        AutomationProperties.SetAutomationId(toggle,
            id + EmulationVideoSettingsLayoutConstants.EnabledAutomationIdSuffix);
        toggle.Checked += (_, _) =>
        {
            if (!_loading) changed(value ?? EmulationVideoProcessingLimits.IntensityMinimum);
            RebuildContent();
        };
        toggle.Unchecked += (_, _) =>
        {
            if (!_loading) changed(null);
            RebuildContent();
        };
        row.Children.Add(Field(ParameterKey(id), toggle));
        if (enabled)
            AddSlider(row, id, value!.Value, EmulationVideoProcessingLimits.IntensityMinimum,
                EmulationVideoProcessingLimits.IntensityMaximum, number => changed(number));
        panel.Children.Add(row);
    }

    private void AddSlider(Panel panel, string id, int value, int minimum, int maximum,
        Action<int> changed)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            TickFrequency = 1,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var displayedValue = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 36,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        slider.ValueChanged += (_, _) =>
        {
            var number = (int)slider.Value;
            displayedValue.Text = number.ToString(CultureInfo.CurrentCulture);
            if (!_loading) changed(number);
        };
        grid.Children.Add(slider);
        Grid.SetColumn(displayedValue, 1);
        grid.Children.Add(displayedValue);
        panel.Children.Add(Field(ParameterKey(id), grid));
    }

    private void AddToggle(Panel panel, string id, bool value, Action<bool> changed)
    {
        var toggle = new CheckBox { IsChecked = value };
        AutomationProperties.SetAutomationId(toggle, id);
        toggle.Checked += (_, _) => { if (!_loading) changed(true); };
        toggle.Unchecked += (_, _) => { if (!_loading) changed(false); };
        panel.Children.Add(Field(ParameterKey(id), toggle));
    }

    private void AddArgb(Panel panel, string id, uint? value, Action<uint?> changed)
    {
        var input = new TextBox
        {
            Text = value?.ToString(
                EmulationVideoSettingsLayoutConstants.ArgbHexadecimalFormat,
                CultureInfo.InvariantCulture) ?? string.Empty
        };
        AutomationProperties.SetAutomationId(input, id);
        input.LostKeyboardFocus += (_, _) =>
        {
            if (uint.TryParse(input.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var color))
                changed(color);
            else if (string.IsNullOrWhiteSpace(input.Text))
                changed(null);
            else
            input.Text = value?.ToString(
                EmulationVideoSettingsLayoutConstants.ArgbHexadecimalFormat,
                CultureInfo.InvariantCulture) ?? string.Empty;
        };
        panel.Children.Add(Field(ParameterKey(id), input));
    }

    private static FrameworkElement Field(string labelResourceKey, FrameworkElement control)
    {
        var field = new StackPanel { Margin = new Thickness(0, 3, 0, 9) };
        field.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(labelResourceKey),
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(control);
        return field;
    }

    private static string ParameterKey(string id) =>
        EmulationVideoProcessingCatalog.ParameterResourceKeys[id];

}
