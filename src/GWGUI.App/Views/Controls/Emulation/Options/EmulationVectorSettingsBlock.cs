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
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationVectorSettingsBlock
{
    internal static FrameworkElement Create(EmulationVectorVideoConfiguration value, Action<Func<EmulationVectorVideoConfiguration, EmulationVectorVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());

        AddGroup(groups, EmulationResourceKeys.VideoVectorGroupDrawing, 0,
            Slider(EmulationVideoProcessingCatalog.VectorLineThreshold, value.LineThreshold, number => changed(current => current with { LineThreshold = number })),
            Slider(EmulationVideoProcessingCatalog.VectorLineIntensity, value.LineIntensity, number => changed(current => current with { LineIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VectorBeamWidth, value.BeamWidth, number => changed(current => current with { BeamWidth = number })),
            Slider(EmulationVideoProcessingCatalog.VectorBeamFocus, value.BeamFocus, number => changed(current => current with { BeamFocus = number })),
            Choice(EmulationVideoProcessingCatalog.VectorPhosphorColor, EmulationVideoProcessingCatalog.CrtColorModeResourceKeys, value.PhosphorColor, color => changed(current => current with { PhosphorColor = color })));
        AddGroup(groups, EmulationResourceKeys.VideoVectorGroupGlow, 1,
            Slider(EmulationVideoProcessingCatalog.VectorHaloIntensity, value.HaloIntensity, number => changed(current => current with { HaloIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.VectorHaloRadius, value.HaloRadius, number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.VectorPersistence, value.PersistenceIntensity, number => changed(current => current with { PersistenceIntensity = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int column, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, 0, column, 1, new Thickness(column == 0 ? 0 : 6, 0, column == 0 ? 6 : 0, 0), fields);

    private static FrameworkElement Slider(string id, int value, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, 100, changed, 1, 36, new Thickness(0, 3, 0, 9));

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
        => EmulationSettingsFieldControlFunctions.CreateVideoChoice(id, resources, selected, changed, 180, new Thickness(0, 3, 0, 9));
}
