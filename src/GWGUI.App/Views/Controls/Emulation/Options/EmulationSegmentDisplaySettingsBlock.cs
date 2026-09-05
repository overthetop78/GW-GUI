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

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationSegmentDisplaySettingsBlock
{
    internal static FrameworkElement Create(EmulationSegmentDisplayVideoConfiguration value,
        Action<Func<EmulationSegmentDisplayVideoConfiguration,
            EmulationSegmentDisplayVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupCells, 0, 0,
            Choice(EmulationVideoProcessingCatalog.SegmentDisplayLayout,
                EmulationVideoProcessingCatalog.SegmentDisplayLayoutResourceKeys, value.Layout,
                choice => changed(current => current with { Layout = choice })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayCellSize, value.CellSize, 100,
                number => changed(current => current with { CellSize = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayHorizontalGap,
                value.HorizontalGap, 100,
                number => changed(current => current with { HorizontalGap = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayVerticalGap,
                value.VerticalGap, 100,
                number => changed(current => current with { VerticalGap = number })));

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupGeometry, 0, 1,
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayThickness, value.Thickness, 100,
                number => changed(current => current with { Thickness = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplaySegmentGap, value.SegmentGap, 100,
                number => changed(current => current with { SegmentGap = number })),
            Choice(EmulationVideoProcessingCatalog.SegmentDisplayEndShape,
                EmulationVideoProcessingCatalog.SegmentDisplayEndShapeResourceKeys, value.EndShape,
                choice => changed(current => current with { EndShape = choice })),
            Check(EmulationVideoProcessingCatalog.SegmentDisplayDecimalPoint, value.DecimalPoint,
                enabled => changed(current => current with { DecimalPoint = enabled })),
            Check(EmulationVideoProcessingCatalog.SegmentDisplayColon, value.Colon,
                enabled => changed(current => current with { Colon = enabled })));

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupEmission, 1, 0,
            Choice(EmulationVideoProcessingCatalog.SegmentDisplayColor,
                EmulationVideoProcessingCatalog.SegmentDisplayColorResourceKeys, value.Color,
                choice => changed(current => current with { Color = choice })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayBrightness, value.Brightness, 100,
                number => changed(current => current with { Brightness = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayActivationThreshold,
                value.ActivationThreshold, 100,
                number => changed(current => current with { ActivationThreshold = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayContrast, value.Contrast, 100,
                number => changed(current => current with { Contrast = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayOffSegmentVisibility,
                value.OffSegmentVisibility, 100,
                number => changed(current => current with { OffSegmentVisibility = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayBlackDepth, value.BlackDepth, 100,
                number => changed(current => current with { BlackDepth = number })));

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupLightAndResponse, 1, 1,
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayGlow, value.Glow, 100,
                number => changed(current => current with { Glow = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayHaloRadius, value.HaloRadius, 100,
                number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayResponseTime,
                value.ResponseTimeMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { ResponseTimeMilliseconds = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayPersistence,
                value.PersistenceMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { PersistenceMilliseconds = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column,
        params FrameworkElement[] fields)
    {
        var content = new StackPanel();
        content.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(titleResourceKey), FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        foreach (var field in fields) content.Children.Add(field);
        var card = new Border
        {
            Child = content,
            Margin = new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6,
                column == 0 ? 6 : 0, row == 0 ? 6 : 0)
        };
        AutomationProperties.SetAutomationId(card, titleResourceKey);
        card.SetResourceReference(FrameworkElement.StyleProperty,
            ControlVisualConstants.CardStyleResource);
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
        groups.Children.Add(card);
    }

    private static FrameworkElement Slider(string id, int value, int maximum,
        Action<int> changed)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = 0, Maximum = maximum, Value = Math.Clamp(value, 0, maximum),
            TickFrequency = maximum == 100 ? 1 : 5, IsSnapToTickEnabled = true
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

    private static FrameworkElement Check(string id, bool value, Action<bool> changed)
    {
        var check = new CheckBox { IsChecked = value };
        AutomationProperties.SetAutomationId(check, id);
        check.Checked += (_, _) => changed(true);
        check.Unchecked += (_, _) => changed(false);
        return Field(id, check);
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
