using System.Windows.Media;

namespace GWGUI.App.Constants.Controls.Visual;

internal static class ControlVisualConstants
{
    internal const int DisplayIdentifierLength = 8;
    internal const string IdentifierFormat = "N";
    internal const string IconFontName = "Segoe MDL2 Assets";
    internal static string EditGlyph => IconGlyphs.Edit;
    internal static string DeleteGlyph => IconGlyphs.Delete;
    internal static string InformationGlyph => IconGlyphs.Information;
    internal static string GameControllerGlyph => IconGlyphs.Controller;
    internal static string HomeGlyph => IconGlyphs.Home;
    internal static string CloseGlyph => IconGlyphs.Close;
    internal const string EmptyValue = "\u2014";
    internal static string AddGlyph => IconGlyphs.Add;
    internal const string DetailSeparator = " \u00B7 ";
    internal const string WarningSymbol = "\u26A0";
    internal const string CardStyleResource = "Card";
    internal const string MainTabItemStyleResource = "MainTabItemStyle";
    internal const string StatusIconButtonStyleResource = "StatusIconButton";
    internal const string CardBrushResource = "CardBrush";
    internal const string ControlBrushResource = "ControlBrush";
    internal const string BorderBrushResource = "BorderBrush";
    internal const string WindowBrushResource = "WindowBrush";
    internal const string MutedTextBrushResource = "MutedTextBrush";
    internal const string AccentBrushResource = "AccentBrush";
    internal const string TextBrushResource = "TextBrush";
    internal const string SyntheticNameBrushResource = "SyntheticNameBrush";
    internal const string ConfigurationResource = "Emulation.Configuration";
    internal const string OpenMachineResource = "Emulation.Machine.Open";
    internal const string MachinesResource = "Emulation.Tab.Machines";
    internal const string WelcomeResource = "Emulation.Welcome.Text";
    internal const string WelcomeTabResource = "Emulation.Tab.Welcome";
    internal const string CloseResource = "Common.Close";
    internal const string BrowseResource = "Common.Browse";

    internal static Color CompatibleForegroundColor { get; } = Color.FromRgb(31, 111, 58);
    internal static Color CompatibleBackgroundColor { get; } = Color.FromRgb(231, 247, 235);
    internal static Color CompatibleBorderColor { get; } = Color.FromRgb(146, 211, 159);
    internal static FontFamily IconFont { get; } = new(IconFontName);
}
