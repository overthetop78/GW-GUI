using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXbox360Chatpad() => new(ControllerArtworkFileNames.Xbox360Chatpad,
            [
                new(EmulationControllerVisualControl.Keyboard, ControllerVisualZoneShape.RoundedRectangle, 15.0d, 28.0d, 70.0d, 43.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 25.8d, 69.7d, 4.6d, 8.3d)
            ]);
}
