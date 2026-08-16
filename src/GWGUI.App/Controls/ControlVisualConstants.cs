using System.Windows.Media;

namespace GWGUI.App.Controls;

internal static class ControlVisualConstants
{
    internal const string IconFontName = "Segoe MDL2 Assets";
    internal const string DeleteGlyph = "\uE74D";
    internal const string InformationGlyph = "\uE946";
    internal const string GameControllerGlyph = "\uE7FC";
    internal const string CloseGlyph = "\uE8BB";
    internal const string EmptyValue = "\u2014";
    internal const string AmigaTitle = "Amiga";

    internal static FontFamily IconFont { get; } = new(IconFontName);
}
