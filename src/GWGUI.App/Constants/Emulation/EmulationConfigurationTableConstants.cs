using System.Windows;

namespace GWGUI.App.Constants.Emulation;

internal static class EmulationConfigurationTableConstants
{
    internal const string TableHeaderTextStyleResource = "TableHeaderText";

    internal static IReadOnlyList<string> HeaderResourceKeys { get; } =
    [
        "Emulation.Configuration.Machine",
        "Emulation.Tab.Cpu",
        "Emulation.Configuration.TotalRam",
        "Emulation.Configuration.Readers",
        "Emulation.Configuration.Peripherals",
        "Emulation.Configuration.Actions"
    ];

    internal static Thickness CellMargin { get; } = new(14, 8, 14, 8);
    internal static Thickness GlyphSpacingMargin { get; } = new(8, 0, 0, 0);
    internal static Thickness HeaderSeparatorThickness { get; } = new(1, 0, 0, 0);
    internal static Thickness RowSeparatorThickness { get; } = new(0, 1, 0, 0);
}
