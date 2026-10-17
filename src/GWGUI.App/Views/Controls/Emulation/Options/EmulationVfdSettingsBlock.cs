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

        AddGroup(groups, EmulationResourceKeys.VideoVfdGroupPhosphorAndGlass, 0,
            Choice(EmulationVideoProcessingCatalog.VfdColor, EmulationVideoProcessingCatalog.VfdColorResourceKeys, value.Color, color => changed(current => current with { Color = color })),
            Slider(EmulationVideoProcessingCatalog.VfdPhosphorIntensity, value.PhosphorIntensity, 100, number => changed(current => current with { PhosphorIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VfdEmissionThreshold, value.EmissionThreshold, 100, number => changed(current => current with { EmissionThreshold = number })),
            Slider(EmulationVideoProcessingCatalog.VfdGlassDarkening, value.GlassDarkening, 100, number => changed(current => current with { GlassDarkening = number })));

        AddGroup(groups, EmulationResourceKeys.VideoVfdGroupAnodes, 1,
            Choice(EmulationVideoProcessingCatalog.VfdStructure,
                EmulationVideoProcessingCatalog.VfdStructureResourceKeys, value.Structure,
                structure =>
                {
                    changed(current => current with { Structure = structure });
                    rebuild();
                }),
            Slider(EmulationVideoProcessingCatalog.VfdCellSize, value.CellSize, 100, number => changed(current => current with { CellSize = number }), value.Structure == EmulationVfdStructure.DotMatrix),
            Slider(EmulationVideoProcessingCatalog.VfdCellGap, value.CellGap, 100, number => changed(current => current with { CellGap = number }), value.Structure == EmulationVfdStructure.DotMatrix));

        AddGroup(groups, EmulationResourceKeys.VideoVfdGroupGlowAndResponse, 2,
            Slider(EmulationVideoProcessingCatalog.VfdHaloIntensity, value.HaloIntensity, 100, number => changed(current => current with { HaloIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VfdHaloRadius, value.HaloRadius, 100, number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.VfdPersistence, value.PersistenceMilliseconds, EmulationVideoProcessingLimits.DurationMaximumMilliseconds, number => changed(current => current with { PersistenceMilliseconds = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int column, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, 0, column, 1, new Thickness(column == 0 ? 0 : 6, 0, column == 2 ? 0 : 6, 0), fields);

    private static FrameworkElement Slider(string id, int value, int maximum, Action<int> changed, bool enabled = true)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, maximum, changed, maximum == 100 ? 1 : 5, 42, new Thickness(0, 3, 0, 9), enabled);

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
        => EmulationSettingsFieldControlFunctions.CreateVideoChoice(id, resources, selected, changed, 160, new Thickness(0, 3, 0, 9));
}
