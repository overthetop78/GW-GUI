using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationVectorSettingsBlock
{
    internal static FrameworkElement Create(EmulationVectorVideoConfiguration value,
        Action<Func<EmulationVectorVideoConfiguration,
            EmulationVectorVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());

        AddGroup(groups, EmulationResourceKeys.VideoVectorGroupDrawing, 0,
            Slider(EmulationVideoProcessingCatalog.VectorLineThreshold,
                value.LineThreshold,
                number => changed(current => current with { LineThreshold = number })),
            Slider(EmulationVideoProcessingCatalog.VectorLineIntensity,
                value.LineIntensity,
                number => changed(current => current with { LineIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VectorBeamWidth,
                value.BeamWidth,
                number => changed(current => current with { BeamWidth = number })),
            Slider(EmulationVideoProcessingCatalog.VectorBeamFocus,
                value.BeamFocus,
                number => changed(current => current with { BeamFocus = number })),
            Choice(EmulationVideoProcessingCatalog.VectorPhosphorColor,
                EmulationVideoProcessingCatalog.CrtColorModeResourceKeys,
                value.PhosphorColor,
                color => changed(current => current with { PhosphorColor = color })));
        AddGroup(groups, EmulationResourceKeys.VideoVectorGroupGlow, 1,
            Slider(EmulationVideoProcessingCatalog.VectorHaloIntensity,
                value.HaloIntensity,
                number => changed(current => current with { HaloIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VectorHaloRadius,
                value.HaloRadius,
                number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.VectorPersistence,
                value.PersistenceIntensity,
                number => changed(current => current with { PersistenceIntensity = number })));
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
            Margin = new Thickness(column == 0 ? 0 : 6, 0,
                column == 0 ? 6 : 0, 0)
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
            Minimum = 0,
            Maximum = 100,
            Value = Math.Clamp(value, 0, 100),
            TickFrequency = 1,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var number = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 36,
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
        row.Children.Add(slider);
        Grid.SetColumn(number, 1);
        row.Children.Add(number);
        var field = new StackPanel { Margin = new Thickness(0, 3, 0, 9) };
        field.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(row);
        return field;
    }

    private static FrameworkElement Choice<T>(string id,
        IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
    {
        var choices = resources.Select(item => new ChoiceValue<T>(item.Key,
            LocExtension.Get(item.Value))).ToArray();
        var selector = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(ChoiceValue<T>.DisplayName),
            SelectedItem = choices.First(item =>
                EqualityComparer<T>.Default.Equals(item.Value, selected)),
            MinWidth = 180
        };
        AutomationProperties.SetAutomationId(selector, id);
        selector.SelectionChanged += (_, _) =>
        {
            if (selector.SelectedItem is ChoiceValue<T> choice) changed(choice.Value);
        };
        var field = new StackPanel { Margin = new Thickness(0, 3, 0, 9) };
        field.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(selector);
        return field;
    }

    private sealed record ChoiceValue<T>(T Value, string DisplayName) where T : struct, Enum;
}
