using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationVfdSettingsBlock
{
    internal static FrameworkElement Create(EmulationVfdVideoConfiguration value,
        Action<Func<EmulationVfdVideoConfiguration, EmulationVfdVideoConfiguration>> changed,
        Action rebuild)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());

        AddGroup(groups, EmulationResourceKeys.VideoVfdGroupPhosphorAndGlass, 0,
            Choice(EmulationVideoProcessingCatalog.VfdColor,
                EmulationVideoProcessingCatalog.VfdColorResourceKeys, value.Color,
                color => changed(current => current with { Color = color })),
            Slider(EmulationVideoProcessingCatalog.VfdPhosphorIntensity,
                value.PhosphorIntensity, 100,
                number => changed(current => current with { PhosphorIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VfdEmissionThreshold,
                value.EmissionThreshold, 100,
                number => changed(current => current with { EmissionThreshold = number })),
            Slider(EmulationVideoProcessingCatalog.VfdGlassDarkening,
                value.GlassDarkening, 100,
                number => changed(current => current with { GlassDarkening = number })));

        AddGroup(groups, EmulationResourceKeys.VideoVfdGroupAnodes, 1,
            Choice(EmulationVideoProcessingCatalog.VfdStructure,
                EmulationVideoProcessingCatalog.VfdStructureResourceKeys, value.Structure,
                structure =>
                {
                    changed(current => current with { Structure = structure });
                    rebuild();
                }),
            Slider(EmulationVideoProcessingCatalog.VfdCellSize, value.CellSize, 100,
                number => changed(current => current with { CellSize = number }),
                value.Structure == EmulationVfdStructure.DotMatrix),
            Slider(EmulationVideoProcessingCatalog.VfdCellGap, value.CellGap, 100,
                number => changed(current => current with { CellGap = number }),
                value.Structure == EmulationVfdStructure.DotMatrix));

        AddGroup(groups, EmulationResourceKeys.VideoVfdGroupGlowAndResponse, 2,
            Slider(EmulationVideoProcessingCatalog.VfdHaloIntensity,
                value.HaloIntensity, 100,
                number => changed(current => current with { HaloIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VfdHaloRadius,
                value.HaloRadius, 100,
                number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.VfdPersistence,
                value.PersistenceMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { PersistenceMilliseconds = number })));
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
            Margin = new Thickness(column == 0 ? 0 : 6, 0, column == 2 ? 0 : 6, 0)
        };
        AutomationProperties.SetAutomationId(card, titleResourceKey);
        card.SetResourceReference(FrameworkElement.StyleProperty,
            ControlVisualConstants.CardStyleResource);
        Grid.SetColumn(card, column);
        groups.Children.Add(card);
    }

    private static FrameworkElement Slider(string id, int value, int maximum,
        Action<int> changed, bool enabled = true)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = 0,
            Maximum = maximum,
            Value = Math.Clamp(value, 0, maximum),
            TickFrequency = maximum == 100 ? 1 : 5,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var number = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 42,
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
        var field = Field(id, row);
        field.IsEnabled = enabled;
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
            MinWidth = 160
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
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(control);
        return field;
    }

    private sealed record ChoiceValue<T>(T Value, string DisplayName) where T : struct, Enum;
}
