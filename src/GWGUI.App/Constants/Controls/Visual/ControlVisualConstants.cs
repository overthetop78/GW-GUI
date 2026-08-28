using System.Windows.Media;

namespace GWGUI.App.Constants.Controls.Visual;

internal static class ControlVisualConstants
{
    internal const int DisplayIdentifierLength = 8;
    internal const string IdentifierFormat = "N";
    internal const string IconFontName = "Segoe MDL2 Assets";
    internal const string EditGlyph = "\uE70F";
    internal const string DeleteGlyph = "\uE74D";
    internal const string InformationGlyph = "\uE946";
    internal const string GameControllerGlyph = "\uE7FC";
    internal const string HomeGlyph = "\uE80F";
    internal const string CloseGlyph = "\uE8BB";
    internal const string EmptyValue = "\u2014";
    internal const string AddGlyph = "\uFF0B";
    internal const string DetailSeparator = " \u00B7 ";
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
    internal const string ConfigurationResource = "Emulation.Configuration";
    internal const string OpenMachineResource = "Emulation.Machine.Open";
    internal const string MachinesResource = "Emulation.Tab.Machines";
    internal const string WelcomeResource = "Emulation.Welcome.Text";
    internal const string WelcomeTabResource = "Emulation.Tab.Welcome";
    internal const string CloseResource = "Common.Close";
    internal const string BrowseResource = "Common.Browse";

    internal static FontFamily IconFont { get; } = new(IconFontName);
}
