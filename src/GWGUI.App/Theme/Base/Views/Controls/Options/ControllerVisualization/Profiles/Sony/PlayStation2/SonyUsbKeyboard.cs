namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    // Keyboard assignments belong to the dedicated keyboard editor, not controller-port buttons.
    private static ProfileDefinition CreateSonyUsbKeyboard() => new(ControllerArtworkFileNames.SonyUsbKeyboard, []);
}
