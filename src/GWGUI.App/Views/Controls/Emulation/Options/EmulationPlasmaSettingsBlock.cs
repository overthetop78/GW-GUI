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

internal static class EmulationPlasmaSettingsBlock
{
    internal static FrameworkElement Create(EmulationPlasmaVideoConfiguration value, Action<Func<EmulationPlasmaVideoConfiguration, EmulationPlasmaVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoPlasmaGroupPanelAndCells, 0, 0,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaCellStructure, value.CellStructure, number => changed(current => current with { CellStructure = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaBlackDepth, value.BlackDepth, number => changed(current => current with { BlackDepth = number })));
        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoPlasmaGroupPhosphors, 0, 1,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaPhosphorIntensity, value.PhosphorIntensity, number => changed(current => current with { PhosphorIntensity = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaGammaResponse, value.GammaResponse, number => changed(current => current with { GammaResponse = number })));
        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoPlasmaGroupLight, 1, 0,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaAutomaticBrightnessLimiter, value.AutomaticBrightnessLimiter, number => changed(current => current with { AutomaticBrightnessLimiter = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaDiffusion, value.Diffusion, number => changed(current => current with { Diffusion = number })));
        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoPlasmaGroupTemporal, 1, 1,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaTemporalDithering, value.TemporalDithering, number => changed(current => current with { TemporalDithering = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.PlasmaPersistence, value.PersistenceIntensity, number => changed(current => current with { PersistenceIntensity = number })));
        return groups;
    }
}
