using GWGUI.App.Constants.Controls.Visual;

namespace GWGUI.App.Constants.Emulation;

internal static class EmulationInputSettingsConstants
{
    internal const string NoneControllerResourceKey = "Emulation.Controller.None";
    internal const string KeyboardControllerId = "Keyboard";
    internal const string MouseControllerId = "Mouse";
    internal static string KeyboardIcon => IconGlyphs.Keyboard;
    internal static string MouseIcon => IconGlyphs.Mouse;
    internal static string ControllerIcon => IconGlyphs.Controller;
    internal static string InformationIcon => IconGlyphs.Information;
}
