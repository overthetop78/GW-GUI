using GWGUI.App.Functions.Views.Emulation.Settings;
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
    internal static FrameworkElement Create(EmulationEPaperVideoConfiguration value, Action<Func<EmulationEPaperVideoConfiguration, EmulationEPaperVideoConfiguration>> changed, Action rebuild)
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
            Slider(EmulationVideoProcessingCatalog.EPaperInkDensity, value.InkDensity, 100, number => changed(current => current with { InkDensity = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperContrast, value.Contrast, 100, number => changed(current => current with { Contrast = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperDithering, value.Dithering, 100, number => changed(current => current with { Dithering = number }))
        };
        if (value.ColorMode == EmulationEPaperColorMode.Color4096)
            ink.Add(Slider(EmulationVideoProcessingCatalog.EPaperColorSaturation, value.ColorSaturation, 100, number => changed(current => current with { ColorSaturation = number })));
        AddGroup(groups, EmulationResourceKeys.VideoEPaperGroupInkAndColor, 0, 0, 1, ink.ToArray());

        AddGroup(groups, EmulationResourceKeys.VideoEPaperGroupPaperSurface, 0, 1, 1,
            Slider(EmulationVideoProcessingCatalog.EPaperPaperBrightness, value.PaperBrightness, 100, number => changed(current => current with { PaperBrightness = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperPaperWarmth, value.PaperWarmth, 100, number => changed(current => current with { PaperWarmth = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperSurfaceTexture, value.SurfaceTexture, 100, number => changed(current => current with { SurfaceTexture = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperEdgeSoftness, value.EdgeSoftness, 100, number => changed(current => current with { EdgeSoftness = number })));

        AddGroup(groups, EmulationResourceKeys.VideoEPaperGroupRefresh, 1, 0, 2,
            Slider(EmulationVideoProcessingCatalog.EPaperRefreshTime,
                value.RefreshTimeMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { RefreshTimeMilliseconds = number })),
            Slider(EmulationVideoProcessingCatalog.EPaperGhosting, value.Ghosting, 100, number => changed(current => current with { Ghosting = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column, int columnSpan, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, row, column, columnSpan, new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6, column + columnSpan >= 2 ? 0 : 6, row == 0 ? 6 : 0), fields);

    private static FrameworkElement Slider(string id, int value, int maximum, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, maximum, changed, maximum == 100 ? 1 : 5, 42, new Thickness(0, 3, 0, 9));

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
        => EmulationSettingsFieldControlFunctions.CreateVideoChoice(id, resources, selected, changed, 160, new Thickness(0, 3, 0, 9));
}
