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
            EmulationSettingsFieldControlFunctions.CreateVideoChoice(EmulationVideoProcessingCatalog.LedMatrixShape, EmulationVideoProcessingCatalog.LedMatrixShapeResourceKeys, value.Shape, shape => changed(current => current with { Shape = shape })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.LedMatrixCellSize, value.CellSize, 100, number => changed(current => current with { CellSize = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.LedMatrixCellGap, value.CellGap, 100, number => changed(current => current with { CellGap = number })));
        AddGroup(groups, EmulationResourceKeys.VideoLedMatrixGroupEmission, 1,
            EmulationSettingsFieldControlFunctions.CreateVideoChoice(EmulationVideoProcessingCatalog.LedMatrixColor, EmulationVideoProcessingCatalog.LedMatrixColorResourceKeys, value.Color, color => changed(current => current with { Color = color })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.LedMatrixBrightness, value.Brightness, 100, number => changed(current => current with { Brightness = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.LedMatrixDiffusion, value.Diffusion, 100, number => changed(current => current with { Diffusion = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.LedMatrixHaloRadius, value.HaloRadius, 100, number => changed(current => current with { HaloRadius = number })),
            EmulationSettingsFieldControlFunctions.CreateVideoSlider(EmulationVideoProcessingCatalog.LedMatrixBlackDepth, value.BlackDepth, 100, number => changed(current => current with { BlackDepth = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int column, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, 0, column, 1, new Thickness(column == 0 ? 0 : 6, 0, column == 1 ? 0 : 6, 0), fields);
}
