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

        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupPanelAndCells, 0, 0,
            Slider(EmulationVideoProcessingCatalog.PlasmaCellStructure, value.CellStructure, number => changed(current => current with { CellStructure = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaBlackDepth, value.BlackDepth, number => changed(current => current with { BlackDepth = number })));
        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupPhosphors, 0, 1,
            Slider(EmulationVideoProcessingCatalog.PlasmaPhosphorIntensity, value.PhosphorIntensity, number => changed(current => current with { PhosphorIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaGammaResponse, value.GammaResponse, number => changed(current => current with { GammaResponse = number })));
        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupLight, 1, 0,
            Slider(EmulationVideoProcessingCatalog.PlasmaAutomaticBrightnessLimiter, value.AutomaticBrightnessLimiter, number => changed(current => current with { AutomaticBrightnessLimiter = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaDiffusion, value.Diffusion, number => changed(current => current with { Diffusion = number })));
        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupTemporal, 1, 1,
            Slider(EmulationVideoProcessingCatalog.PlasmaTemporalDithering, value.TemporalDithering, number => changed(current => current with { TemporalDithering = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaPersistence, value.PersistenceIntensity, number => changed(current => current with { PersistenceIntensity = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, row, column, 1, new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6, column == 0 ? 6 : 0, row == 0 ? 6 : 0), fields);

    private static FrameworkElement Slider(string id, int value, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, 100, changed, 1, 36, new Thickness(0, 3, 0, 9));
}
