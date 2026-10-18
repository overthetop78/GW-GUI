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
            EmulationSettingsFieldControlFunctions.CreateVideoChoice(EmulationVideoProcessingCatalog.EPaperColorMode,
                EmulationVideoProcessingCatalog.EPaperColorModeResourceKeys, value.ColorMode,
                choice =>
                {
                    changed(current => current with { ColorMode = choice });
                    rebuild();
                }),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperInkDensity, value.InkDensity, 100, number => changed(current => current with { InkDensity = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperContrast, value.Contrast, 100, number => changed(current => current with { Contrast = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperDithering, value.Dithering, 100, number => changed(current => current with { Dithering = number }))
        };
        if (value.ColorMode == EmulationEPaperColorMode.Color4096)
            ink.Add(EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperColorSaturation, value.ColorSaturation, 100, number => changed(current => current with { ColorSaturation = number })));
        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoEPaperGroupInkAndColor, 0, 0, 1, ink.ToArray());

        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoEPaperGroupPaperSurface, 0, 1, 1,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperPaperBrightness, value.PaperBrightness, 100, number => changed(current => current with { PaperBrightness = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperPaperWarmth, value.PaperWarmth, 100, number => changed(current => current with { PaperWarmth = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperSurfaceTexture, value.SurfaceTexture, 100, number => changed(current => current with { SurfaceTexture = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperEdgeSoftness, value.EdgeSoftness, 100, number => changed(current => current with { EdgeSoftness = number })));

        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoEPaperGroupRefresh, 1, 0, 2,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperRefreshTime,
                value.RefreshTimeMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { RefreshTimeMilliseconds = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.EPaperGhosting, value.Ghosting, 100, number => changed(current => current with { Ghosting = number })));
        return groups;
    }
}
