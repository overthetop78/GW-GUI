using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationLedMatrixSettingsBlock
{
    internal static FrameworkElement Create(EmulationLedMatrixVideoConfiguration value,
        Action<Func<EmulationLedMatrixVideoConfiguration,
            EmulationLedMatrixVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        AddGroup(groups, EmulationResourceKeys.VideoLedMatrixGroupStructure, 0,
            Choice(EmulationVideoProcessingCatalog.LedMatrixShape,
                EmulationVideoProcessingCatalog.LedMatrixShapeResourceKeys, value.Shape,
                shape => changed(current => current with { Shape = shape })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixCellSize, value.CellSize,
                number => changed(current => current with { CellSize = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixCellGap, value.CellGap,
                number => changed(current => current with { CellGap = number })));
        AddGroup(groups, EmulationResourceKeys.VideoLedMatrixGroupEmission, 1,
            Choice(EmulationVideoProcessingCatalog.LedMatrixColor,
                EmulationVideoProcessingCatalog.LedMatrixColorResourceKeys, value.Color,
                color => changed(current => current with { Color = color })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixBrightness, value.Brightness,
                number => changed(current => current with { Brightness = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixDiffusion, value.Diffusion,
                number => changed(current => current with { Diffusion = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixHaloRadius, value.HaloRadius,
                number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixBlackDepth, value.BlackDepth,
                number => changed(current => current with { BlackDepth = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int column,
        params FrameworkElement[] fields)
    {
        var content = new StackPanel();
        content.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(titleResourceKey),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        foreach (var field in fields) content.Children.Add(field);
        var card = new Border
        {
            Child = content,
            Margin = new Thickness(column == 0 ? 0 : 6, 0, column == 1 ? 0 : 6, 0)
        };
        AutomationProperties.SetAutomationId(card, titleResourceKey);
        card.SetResourceReference(FrameworkElement.StyleProperty,
            ControlVisualConstants.CardStyleResource);
        Grid.SetColumn(card, column);
        groups.Children.Add(card);
    }

    private static FrameworkElement Slider(string id, int value, Action<int> changed)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = 0, Maximum = 100, Value = Math.Clamp(value, 0, 100),
            TickFrequency = 1, IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var number = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture), MinWidth = 42,
            TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        slider.ValueChanged += (_, _) =>
        {
            var current = (int)slider.Value;
            number.Text = current.ToString(CultureInfo.CurrentCulture);
            changed(current);
        };
        row.Children.Add(slider);
        Grid.SetColumn(number, 1);
        row.Children.Add(number);
        return Field(id, row);
    }

    private static FrameworkElement Choice<T>(string id,
        IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
    {
        var choices = resources.Select(item => new ChoiceValue<T>(item.Key,
            LocExtension.Get(item.Value))).ToArray();
        var selector = new ComboBox
        {
            ItemsSource = choices, DisplayMemberPath = nameof(ChoiceValue<T>.DisplayName),
            SelectedItem = choices.First(item =>
                EqualityComparer<T>.Default.Equals(item.Value, selected)), MinWidth = 160
        };
        AutomationProperties.SetAutomationId(selector, id);
        selector.SelectionChanged += (_, _) =>
        {
            if (selector.SelectedItem is ChoiceValue<T> choice) changed(choice.Value);
        };
        return Field(id, selector);
    }

    private static FrameworkElement Field(string id, FrameworkElement control)
    {
        var field = new StackPanel { Margin = new Thickness(0, 3, 0, 9) };
        field.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            Margin = new Thickness(0, 0, 0, 4), TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(control);
        return field;
    }

    private sealed record ChoiceValue<T>(T Value, string DisplayName) where T : struct, Enum;
}
