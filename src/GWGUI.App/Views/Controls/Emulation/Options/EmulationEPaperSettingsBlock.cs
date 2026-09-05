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

internal static class EmulationEPaperSettingsBlock
{
    internal static FrameworkElement Create(EmulationEPaperVideoConfiguration value,
        Action<Func<EmulationEPaperVideoConfiguration, EmulationEPaperVideoConfiguration>> changed,
        Action rebuild)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var ink = new List<FrameworkElement>
        {
            Choice(EmulationVideoProcessingCatalog.EPaperColorMode,
                EmulationVideoProcessingCatalog.EPaperColorModeResourceKeys, value.ColorMode,
                choice =>
                {
                    changed(current => current with { ColorMode = choice });
                    rebuild();
                }),
            Slider(EmulationVideoProcessingCatalog.EPaperInkDensity, value.InkDensity, 100,
                number => changed(current => current with { InkDensity = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperContrast, value.Contrast, 100,
                number => changed(current => current with { Contrast = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperDithering, value.Dithering, 100,
                number => changed(current => current with { Dithering = number }))
        };
        if (value.ColorMode == EmulationEPaperColorMode.Color4096)
            ink.Add(Slider(EmulationVideoProcessingCatalog.EPaperColorSaturation,
                value.ColorSaturation, 100,
                number => changed(current => current with { ColorSaturation = number })));
        AddGroup(groups, EmulationResourceKeys.VideoEPaperGroupInkAndColor, 0, 0, 1,
            ink.ToArray());

        AddGroup(groups, EmulationResourceKeys.VideoEPaperGroupPaperSurface, 0, 1, 1,
            Slider(EmulationVideoProcessingCatalog.EPaperPaperBrightness,
                value.PaperBrightness, 100,
                number => changed(current => current with { PaperBrightness = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperPaperWarmth, value.PaperWarmth, 100,
                number => changed(current => current with { PaperWarmth = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperSurfaceTexture,
                value.SurfaceTexture, 100,
                number => changed(current => current with { SurfaceTexture = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperEdgeSoftness,
                value.EdgeSoftness, 100,
                number => changed(current => current with { EdgeSoftness = number })));

        AddGroup(groups, EmulationResourceKeys.VideoEPaperGroupRefresh, 1, 0, 2,
            Slider(EmulationVideoProcessingCatalog.EPaperRefreshTime,
                value.RefreshTimeMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { RefreshTimeMilliseconds = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperGhosting, value.Ghosting, 100,
                number => changed(current => current with { Ghosting = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column,
        int columnSpan, params FrameworkElement[] fields)
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
                column + columnSpan >= 2 ? 0 : 6, row == 0 ? 6 : 0)
        };
        AutomationProperties.SetAutomationId(card, titleResourceKey);
        card.SetResourceReference(FrameworkElement.StyleProperty,
            ControlVisualConstants.CardStyleResource);
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
        Grid.SetColumnSpan(card, columnSpan);
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
