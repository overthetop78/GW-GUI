using GWGUI.App.Functions.Views.Emulation.Settings;
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
    internal static FrameworkElement Create(EmulationFixedPixelVideoConfiguration value, Action<Func<EmulationFixedPixelVideoConfiguration, EmulationFixedPixelVideoConfiguration>> changed, Action rebuild)
    {
        var root = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
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
                changed(current => current with { Technology = technology });
                rebuild();
            }));
        screen.Children.Add(Choice(EmulationVideoProcessingCatalog.FixedPixelSubpixels,
            EmulationVideoProcessingCatalog.SubpixelLayoutResourceKeys, value.Subpixels,
            subpixels =>
            {
                changed(current => current with { Subpixels = subpixels });
                rebuild();
            }));
        if (value.Subpixels == EmulationSubpixelLayout.Monochrome)
            screen.Children.Add(Choice(EmulationVideoProcessingCatalog.FixedPixelMonochromeColor,
                EmulationVideoProcessingCatalog.MonochromePaletteResourceKeys,
                value.MonochromePalette,
                palette => changed(current => current with { MonochromePalette = palette })));
        AddGroup(groups, screen, 0, 0);

        var structure = new StackPanel();
        structure.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelGridIntensity, value.GridIntensity, 0, 100, number => changed(current => current with { GridIntensity = number })));
        structure.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelPixelGap, value.PixelGap, 0, 100, number => changed(current => current with { PixelGap = number })));
        AddGroup(groups, structure, 0, 1);

        var light = new StackPanel();
        if (value.Technology != EmulationFixedPixelTechnology.Oled)
        {
            light.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelBacklight,
                value.BacklightIntensity ?? (value.Technology == EmulationFixedPixelTechnology.Lcd ? 65 : 80),
                0, 100, number => changed(current => current with { BacklightIntensity = number })));
            light.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelBacklightBleed, value.BacklightBleedIntensity, 0, 100, number => changed(current => current with { BacklightBleedIntensity = number })));
        }
        light.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelBlackDepth,
            value.BlackDepth ?? value.Technology switch
            {
                EmulationFixedPixelTechnology.Lcd => 35,
                EmulationFixedPixelTechnology.LedBacklitLcd => 55,
                _ => 100
            }, 0, 100, number => changed(current => current with { BlackDepth = number })));
        AddGroup(groups, light, 1, 0);

        var temporal = new StackPanel();
        temporal.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelResponseTime,
            value.ResponseTimeMilliseconds, EmulationVideoProcessingLimits.DurationMinimumMilliseconds,
            EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
            number => changed(current => current with { ResponseTimeMilliseconds = number })));
        temporal.Children.Add(Slider(EmulationVideoProcessingCatalog.FixedPixelPersistence, value.PersistenceIntensity, 0, 100, number => changed(current => current with { PersistenceIntensity = number })));
        AddGroup(groups, temporal, 1, 1);

        root.Children.Add(groups);
        return root;
    }

    private static void AddGroup(Grid grid, FrameworkElement content, int row, int column)
        => EmulationSettingsLayout.AddVideoProcessingGroup(grid, null, row, column, 1, new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6, column == 0 ? 6 : 0, row == 0 ? 6 : 0), content);

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed) where T : struct, Enum
        => EmulationSettingsFieldControlFunctions.CreateVideoChoice(id, resources, selected, changed, 180, new Thickness(0, 2, 0, 8));

    private static FrameworkElement Slider(string id, int value, int minimum, int maximum, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, minimum, maximum, changed, 1, 38, new Thickness(0, 2, 0, 8));
}
