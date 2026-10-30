using GWGUI.App.Functions.Views.Emulation.Settings;
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

internal static class EmulationProjectionSettingsBlock
{
    internal static FrameworkElement Create(EmulationProjectionVideoConfiguration value, Action<Func<EmulationProjectionVideoConfiguration, EmulationProjectionVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoProjectionGroupOptics, 0, 0, 1,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.ProjectionOpticalBlur, value.OpticalBlur, 100, number => changed(current => current with { OpticalBlur = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.ProjectionDiffusion, value.Diffusion, 100, number => changed(current => current with { Diffusion = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.ProjectionConvergence, value.Convergence, 100, number => changed(current => current with { Convergence = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.ProjectionLightOutput, value.LightOutput, 100, number => changed(current => current with { LightOutput = number })));
        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoProjectionGroupScreen, 0, 1, 1,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.ProjectionScreenTexture, value.ScreenTexture, 100, number => changed(current => current with { ScreenTexture = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.ProjectionAmbientLight, value.AmbientLight, 100, number => changed(current => current with { AmbientLight = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.ProjectionVignette, value.Vignette, 100, number => changed(current => current with { Vignette = number })));
        return groups;
    }

}
