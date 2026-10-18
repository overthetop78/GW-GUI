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

internal static class EmulationVfdSettingsBlock
{
    internal static FrameworkElement Create(EmulationVfdVideoConfiguration value, Action<Func<EmulationVfdVideoConfiguration, EmulationVfdVideoConfiguration>> changed, Action rebuild)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());

        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoVfdGroupPhosphorAndGlass, 0,
            EmulationSettingsFieldControlFunctions.CreateVideoChoice(EmulationVideoProcessingCatalog.VfdColor, EmulationVideoProcessingCatalog.VfdColorResourceKeys, value.Color, color => changed(current => current with { Color = color })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdPhosphorIntensity, value.PhosphorIntensity, 100, number => changed(current => current with { PhosphorIntensity = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdEmissionThreshold, value.EmissionThreshold, 100, number => changed(current => current with { EmissionThreshold = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdGlassDarkening, value.GlassDarkening, 100, number => changed(current => current with { GlassDarkening = number })));

        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoVfdGroupAnodes, 1,
            EmulationSettingsFieldControlFunctions.CreateVideoChoice(EmulationVideoProcessingCatalog.VfdStructure,
                EmulationVideoProcessingCatalog.VfdStructureResourceKeys, value.Structure,
                structure =>
                {
                    changed(current => current with { Structure = structure });
                    rebuild();
                }),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdCellSize, value.CellSize, 100, number => changed(current => current with { CellSize = number }), value.Structure == EmulationVfdStructure.DotMatrix),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdCellGap, value.CellGap, 100, number => changed(current => current with { CellGap = number }), value.Structure == EmulationVfdStructure.DotMatrix));

        EmulationSettingsLayout.AddVideoProcessingGroup(groups, EmulationResourceKeys.VideoVfdGroupGlowAndResponse, 2,
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdHaloIntensity, value.HaloIntensity, 100, number => changed(current => current with { HaloIntensity = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdHaloRadius, value.HaloRadius, 100, number => changed(current => current with { HaloRadius = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.VfdPersistence, value.PersistenceMilliseconds, EmulationVideoProcessingLimits.DurationMaximumMilliseconds, number => changed(current => current with { PersistenceMilliseconds = number })));
        return groups;
    }
}
