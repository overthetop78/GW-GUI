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
        AddGroup(groups, EmulationResourceKeys.VideoProjectionGroupOptics, 0, 0, 1,
            Slider(EmulationVideoProcessingCatalog.ProjectionOpticalBlur, value.OpticalBlur, 100, number => changed(current => current with { OpticalBlur = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionDiffusion, value.Diffusion, 100, number => changed(current => current with { Diffusion = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionConvergence, value.Convergence, 100, number => changed(current => current with { Convergence = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionLightOutput, value.LightOutput, 100, number => changed(current => current with { LightOutput = number })));
        AddGroup(groups, EmulationResourceKeys.VideoProjectionGroupScreen, 0, 1, 1,
            Slider(EmulationVideoProcessingCatalog.ProjectionScreenTexture, value.ScreenTexture, 100, number => changed(current => current with { ScreenTexture = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionAmbientLight, value.AmbientLight, 100, number => changed(current => current with { AmbientLight = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionVignette, value.Vignette, 100, number => changed(current => current with { Vignette = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column, int columnSpan, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, row, column, columnSpan, new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6, column + columnSpan >= 2 ? 0 : 6, row == 0 ? 6 : 0), fields);

    private static FrameworkElement Slider(string id, int value, int maximum, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, maximum, changed, maximum == 100 ? 1 : 5, 42, new Thickness(0, 3, 0, 9));

}
