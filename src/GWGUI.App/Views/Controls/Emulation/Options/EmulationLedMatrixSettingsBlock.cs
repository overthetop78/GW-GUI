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

internal static class EmulationLedMatrixSettingsBlock
{
    internal static FrameworkElement Create(EmulationLedMatrixVideoConfiguration value, Action<Func<EmulationLedMatrixVideoConfiguration, EmulationLedMatrixVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        AddGroup(groups, EmulationResourceKeys.VideoLedMatrixGroupStructure, 0,
            Choice(EmulationVideoProcessingCatalog.LedMatrixShape, EmulationVideoProcessingCatalog.LedMatrixShapeResourceKeys, value.Shape, shape => changed(current => current with { Shape = shape })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixCellSize, value.CellSize, number => changed(current => current with { CellSize = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixCellGap, value.CellGap, number => changed(current => current with { CellGap = number })));
        AddGroup(groups, EmulationResourceKeys.VideoLedMatrixGroupEmission, 1,
            Choice(EmulationVideoProcessingCatalog.LedMatrixColor, EmulationVideoProcessingCatalog.LedMatrixColorResourceKeys, value.Color, color => changed(current => current with { Color = color })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixBrightness, value.Brightness, number => changed(current => current with { Brightness = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixDiffusion, value.Diffusion, number => changed(current => current with { Diffusion = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixHaloRadius, value.HaloRadius, number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.LedMatrixBlackDepth, value.BlackDepth, number => changed(current => current with { BlackDepth = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int column, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, 0, column, 1, new Thickness(column == 0 ? 0 : 6, 0, column == 1 ? 0 : 6, 0), fields);

    private static FrameworkElement Slider(string id, int value, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, 100, changed, 1, 42, new Thickness(0, 3, 0, 9));

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
        => EmulationSettingsFieldControlFunctions.CreateVideoChoice(id, resources, selected, changed, 160, new Thickness(0, 3, 0, 9));
}
