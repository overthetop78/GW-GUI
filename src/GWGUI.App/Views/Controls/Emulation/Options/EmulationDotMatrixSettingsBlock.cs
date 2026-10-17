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

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationDotMatrixSettingsBlock
{
    internal static FrameworkElement Create(EmulationDotMatrixVideoConfiguration value, Action<Func<EmulationDotMatrixVideoConfiguration, EmulationDotMatrixVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        AddGroup(groups, EmulationResourceKeys.VideoDotMatrixGroupCells, 0,
            Choice(EmulationVideoProcessingCatalog.DotMatrixShape, EmulationVideoProcessingCatalog.DotMatrixShapeResourceKeys, value.Shape, shape => changed(current => current with { Shape = shape })),
            Slider(EmulationVideoProcessingCatalog.DotMatrixCellSize, value.CellSize, 100, number => changed(current => current with { CellSize = number })),
            Slider(EmulationVideoProcessingCatalog.DotMatrixDotSize, value.DotSize, 100, number => changed(current => current with { DotSize = number })),
            Slider(EmulationVideoProcessingCatalog.DotMatrixCellGap, value.CellGap, 100, number => changed(current => current with { CellGap = number })));
        AddGroup(groups, EmulationResourceKeys.VideoDotMatrixGroupLight, 1,
            Choice(EmulationVideoProcessingCatalog.DotMatrixPalette, EmulationVideoProcessingCatalog.DotMatrixPaletteResourceKeys, value.Palette, palette => changed(current => current with { Palette = palette })),
            Slider(EmulationVideoProcessingCatalog.DotMatrixBrightness, value.Brightness, 100, number => changed(current => current with { Brightness = number })),
            Slider(EmulationVideoProcessingCatalog.DotMatrixContrast, value.Contrast, 100, number => changed(current => current with { Contrast = number })),
            Slider(EmulationVideoProcessingCatalog.DotMatrixHaloIntensity, value.HaloIntensity, 100, number => changed(current => current with { HaloIntensity = number })));
        AddGroup(groups, EmulationResourceKeys.VideoDotMatrixGroupTemporal, 2,
            Slider(EmulationVideoProcessingCatalog.DotMatrixResponseTime,
                value.ResponseTimeMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { ResponseTimeMilliseconds = number })),
            Slider(EmulationVideoProcessingCatalog.DotMatrixPersistence,
                value.PersistenceMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { PersistenceMilliseconds = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int column, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, 0, column, 1, new Thickness(column == 0 ? 0 : 6, 0, column == 2 ? 0 : 6, 0), fields);

    private static FrameworkElement Slider(string id, int value, int maximum, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, maximum, changed, maximum == 100 ? 1 : 5, 42, new Thickness(0, 3, 0, 9));

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
        => EmulationSettingsFieldControlFunctions.CreateVideoChoice(id, resources, selected, changed, 160, new Thickness(0, 3, 0, 9));
}
