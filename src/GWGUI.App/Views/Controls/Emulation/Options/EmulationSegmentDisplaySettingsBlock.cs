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

internal static class EmulationSegmentDisplaySettingsBlock
{
    internal static FrameworkElement Create(EmulationSegmentDisplayVideoConfiguration value, Action<Func<EmulationSegmentDisplayVideoConfiguration, EmulationSegmentDisplayVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupCells, 0, 0,
            Choice(EmulationVideoProcessingCatalog.SegmentDisplayLayout, EmulationVideoProcessingCatalog.SegmentDisplayLayoutResourceKeys, value.Layout, choice => changed(current => current with { Layout = choice })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayCellSize, value.CellSize, 100, number => changed(current => current with { CellSize = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayHorizontalGap, value.HorizontalGap, 100, number => changed(current => current with { HorizontalGap = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayVerticalGap, value.VerticalGap, 100, number => changed(current => current with { VerticalGap = number })));

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupGeometry, 0, 1,
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayThickness, value.Thickness, 100, number => changed(current => current with { Thickness = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplaySegmentGap, value.SegmentGap, 100, number => changed(current => current with { SegmentGap = number })),
            Choice(EmulationVideoProcessingCatalog.SegmentDisplayEndShape, EmulationVideoProcessingCatalog.SegmentDisplayEndShapeResourceKeys, value.EndShape, choice => changed(current => current with { EndShape = choice })),
            Check(EmulationVideoProcessingCatalog.SegmentDisplayDecimalPoint, value.DecimalPoint, enabled => changed(current => current with { DecimalPoint = enabled })),
            Check(EmulationVideoProcessingCatalog.SegmentDisplayColon, value.Colon, enabled => changed(current => current with { Colon = enabled })));

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupEmission, 1, 0,
            Choice(EmulationVideoProcessingCatalog.SegmentDisplayColor, EmulationVideoProcessingCatalog.SegmentDisplayColorResourceKeys, value.Color, choice => changed(current => current with { Color = choice })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayBrightness, value.Brightness, 100, number => changed(current => current with { Brightness = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayActivationThreshold, value.ActivationThreshold, 100, number => changed(current => current with { ActivationThreshold = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayContrast, value.Contrast, 100, number => changed(current => current with { Contrast = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayOffSegmentVisibility, value.OffSegmentVisibility, 100, number => changed(current => current with { OffSegmentVisibility = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayBlackDepth, value.BlackDepth, 100, number => changed(current => current with { BlackDepth = number })));

        AddGroup(groups, EmulationResourceKeys.VideoSegmentDisplayGroupLightAndResponse, 1, 1,
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayGlow, value.Glow, 100, number => changed(current => current with { Glow = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayHaloRadius, value.HaloRadius, 100, number => changed(current => current with { HaloRadius = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayResponseTime,
                value.ResponseTimeMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { ResponseTimeMilliseconds = number })),
            Slider(EmulationVideoProcessingCatalog.SegmentDisplayPersistence,
                value.PersistenceMilliseconds,
                EmulationVideoProcessingLimits.DurationMaximumMilliseconds,
                number => changed(current => current with { PersistenceMilliseconds = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column, params FrameworkElement[] fields)
        => EmulationSettingsLayout.AddVideoProcessingGroup(groups, titleResourceKey, row, column, 1, new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6, column == 0 ? 6 : 0, row == 0 ? 6 : 0), fields);

    private static FrameworkElement Slider(string id, int value, int maximum, Action<int> changed)
        => EmulationSettingsFieldControlFunctions.CreateVideoSlider(id, value, 0, maximum, changed, maximum == 100 ? 1 : 5, 42, new Thickness(0, 3, 0, 9));

    private static FrameworkElement Check(string id, bool value, Action<bool> changed)
    {
        var check = new CheckBox { IsChecked = value };
        AutomationProperties.SetAutomationId(check, id);
        check.Checked += (_, _) => changed(true);
        check.Unchecked += (_, _) => changed(false);
        return EmulationSettingsFieldControlFunctions.CreateVideoField(id, check, new Thickness(0, 3, 0, 9));
    }

    private static FrameworkElement Choice<T>(string id, IReadOnlyDictionary<T, string> resources, T selected, Action<T> changed)
        where T : struct, Enum
        => EmulationSettingsFieldControlFunctions.CreateVideoChoice(id, resources, selected, changed, 160, new Thickness(0, 3, 0, 9));
}
