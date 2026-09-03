using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationFixedPixelSettingsBlock
{
    internal static FrameworkElement Create(EmulationFixedPixelVideoConfiguration value,
        Action<EmulationFixedPixelVideoConfiguration> changed, Action rebuild)
    {
        var root = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        root.Children.Add(new TextBlock
        {
            Text = LocExtension.Get("Emulation.Video.Technology.FixedPixel"),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var screen = new StackPanel();
        screen.Children.Add(Choice(EmulationVideoProcessingCatalog.FixedPixelTechnology,
            EmulationVideoProcessingCatalog.FixedPixelTechnologyResourceKeys, value.Technology,
            technology =>
            {
                changed(value with { Technology = technology });
                rebuild();
            }));
        screen.Children.Add(Choice(EmulationVideoProcessingCatalog.FixedPixelSubpixels,
            EmulationVideoProcessingCatalog.SubpixelLayoutResourceKeys, value.Subpixels,
            subpixels =>
            {
                changed(value with { Subpixels = subpixels });
                rebuild();
            }));
        if (value.Subpixels == EmulationSubpixelLayout.Monochrome)
            screen.Children.Add(Choice(EmulationVideoProcessingCatalog.FixedPixelMonochromeColor,
                EmulationVideoProcessingCatalog.MonochromePaletteResourceKeys,
                value.MonochromePalette,
                palette => changed(value with { MonochromePalette = palette, MonochromeColorArgb = null })));
        AddGroup(groups, screen, EmulationVideoProcessingCatalog.FixedPixelTechnology, 0, 0);

        var structure = new StackPanel();
        structure.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelGridIntensity,
            value.GridIntensity, 0, 100,
            number => changed(value with { GridIntensity = number })));
        structure.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelPixelGap,
            value.PixelGap, 0, 100,
            number => changed(value with { PixelGap = number })));
        AddGroup(groups, structure, EmulationVideoProcessingCatalog.FixedPixelGridIntensity, 0, 1);

        var light = new StackPanel();
        if (value.Technology != EmulationFixedPixelTechnology.Oled)
        {
            light.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelBacklight,
                value.BacklightIntensity ?? (value.Technology == EmulationFixedPixelTechnology.Lcd ? 65 : 80),
                0, 100, number => changed(value with { BacklightIntensity = number })));
            light.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelBacklightBleed,
                value.BacklightBleedIntensity, 0, 100,
                number => changed(value with { BacklightBleedIntensity = number })));
        }
        light.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelBlackDepth,
            value.BlackDepth ?? value.Technology switch
            {
                EmulationFixedPixelTechnology.Lcd => 35,
                EmulationFixedPixelTechnology.LedBacklitLcd => 55,
                _ => 100
            }, 0, 100, number => changed(value with { BlackDepth = number })));
        AddGroup(groups, light, value.Technology == EmulationFixedPixelTechnology.Oled
            ? EmulationVideoProcessingCatalog.FixedPixelBlackDepth
            : EmulationVideoProcessingCatalog.FixedPixelBacklight, 1, 0);

        var temporal = new StackPanel();
        temporal.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelResponseTime,
            value.ResponseTimeMilliseconds, EmulationVideoProcessingLimits.DurationMinimumMilliseconds,
            EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
            number => changed(value with { ResponseTimeMilliseconds = number })));
        temporal.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelPersistence,
            value.PersistenceIntensity, 0, 100,
            number => changed(value with { PersistenceIntensity = number })));
        AddGroup(groups, temporal, EmulationVideoProcessingCatalog.FixedPixelResponseTime, 1, 1);

        root.Children.Add(groups);
        return root;
    }

    private static void AddGroup(Grid grid, FrameworkElement content, string titleId, int row, int column)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[titleId]),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        panel.Children.Add(content);
        var card = new Border
        {
            Child = panel,
            Margin = new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6,
                column == 0 ? 6 : 0, row == 0 ? 6 : 0)
        };
        card.SetResourceReference(FrameworkElement.StyleProperty, ControlVisualConstants.CardStyleResource);
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
        grid.Children.Add(card);
    }

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources,
        T selected, Action<T> changed) where T : struct, Enum
    {
        var choices = resources.Select(item => new ChoiceValue<T>(item.Key,
            LocExtension.Get(item.Value))).ToArray();
        var selector = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(ChoiceValue<T>.DisplayName),
            SelectedItem = choices.First(item => EqualityComparer<T>.Default.Equals(item.Value, selected)),
            MinWidth = 180
        };
        AutomationProperties.SetAutomationId(selector, id);
        selector.SelectionChanged += (_, _) =>
        {
            if (selector.SelectedItem is ChoiceValue<T> choice) changed(choice.Value);
        };
        return Field(id, selector);
    }

    private static FrameworkElement Slider(string id, int value, int minimum, int maximum,
        Action<int> changed)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = Math.Clamp(value, minimum, maximum),
            TickFrequency = 1,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var number = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 38,
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
        return Field(id, grid);
    }

    private static FrameworkElement Field(string id, FrameworkElement control)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 2, 0, 8) };
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
