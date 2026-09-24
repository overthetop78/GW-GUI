using GWGUI.App.Constants.Controls.Visual;

namespace GWGUI.App.Constants.Emulation;

internal static class EmulationFirmwareSettingsConstants
{
    internal static string RefreshIcon => IconGlyphs.Refresh;
    internal static string OpenFolderIcon => IconGlyphs.OpenFolder;
    internal static string FirmwareIcon => IconGlyphs.Processor;
    internal const double FirmwareRowMinimumHeight = 66;
    internal const double FirmwareIconColumnWidth = 44;
    internal const string FirmwareBadgeSharedSizeGroup = "FirmwareBadges";
    internal const double FirmwareBadgeSpacing = 8;
    internal const int FirmwareDestinationMaximumLength = 20;
    internal const string FirmwareDestinationEllipsis = "\u2026";
}
