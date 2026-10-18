using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Dictionaries;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsFieldControlFunctions
{
    internal static FrameworkElement CreateVideoSlider(string id, int value, int maximum,
        Action<int> changed, bool enabled = true)
        => CreateVideoSlider(id, value, 0, maximum, changed, maximum == 100 ? 1 : 5,
            42, new Thickness(0, 3, 0, 9), enabled);

    internal static FrameworkElement CreateVideoChoice<T>(string id,
        IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed) where T : struct, Enum
        => CreateVideoChoice(id, resources, selected, changed, 160, new Thickness(0, 3, 0, 9));

    internal static FrameworkElement CreateVideoSlider(string id, int value, int minimum, int maximum, Action<int> changed, double tickFrequency, double labelMinimumWidth, Thickness fieldMargin, bool enabled = true)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = Math.Clamp(value, minimum, maximum),
            TickFrequency = tickFrequency,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var number = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = labelMinimumWidth,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        slider.ValueChanged += (_, _) =>
        {
            var current = (int)slider.Value;
            number.Text = current.ToString(CultureInfo.CurrentCulture);
            changed(current);
        };
        grid.Children.Add(slider);
        Grid.SetColumn(number, 1);
        grid.Children.Add(number);
        var field = CreateVideoField(id, grid, fieldMargin);
        field.IsEnabled = enabled;
        return field;
    }
    internal static FrameworkElement CreateVideoChoice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed, double minimumWidth, Thickness fieldMargin) where T : struct, Enum
    {
        var choices = resources.Select(item => new ChoiceValue<T>(item.Key, LocExtension.Get(item.Value))).ToArray();
        var selector = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(ChoiceValue<T>.DisplayName),
            SelectedItem = choices.First(item => EqualityComparer<T>.Default.Equals(item.Value, selected)),
            MinWidth = minimumWidth
        };
        AutomationProperties.SetAutomationId(selector, id);
        selector.SelectionChanged += (_, _) =>
        {
            if (selector.SelectedItem is ChoiceValue<T> choice) changed(choice.Value);
        };
        return CreateVideoField(id, selector, fieldMargin);
    }
    internal static FrameworkElement CreateVideoField(string id, FrameworkElement control, Thickness margin)
    {
        var panel = new StackPanel { Margin = margin };
        panel.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(control);
        return panel;
    }
    private sealed record ChoiceValue<T>(T Value, string DisplayName) where T : struct, Enum;
}
