using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXboxChatpad() => new(ControllerArtworkFileNames.XboxChatpad,
            [
                new(EmulationControllerVisualControl.Keyboard, ControllerVisualZoneShape.RoundedRectangle, 17.0d, 28.0d, 66.0d, 51.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 47.2d, 80.2d, 5.8d, 8.6d)
            ]);
}
