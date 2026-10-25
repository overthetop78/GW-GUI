using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyKramerStriker() => new(ControllerArtworkFileNames.SonyKramerStriker,
    [
        new(EmulationControllerVisualControl.FretGreen, ControllerVisualZoneShape.RoundedRectangle, 46.15, 12.89, 8.6, 1.95),
        new(EmulationControllerVisualControl.FretRed, ControllerVisualZoneShape.RoundedRectangle, 46.15, 15.62, 8.6, 1.95),
        new(EmulationControllerVisualControl.FretYellow, ControllerVisualZoneShape.RoundedRectangle, 46.15, 18.55, 8.6, 1.95),
        new(EmulationControllerVisualControl.FretBlue, ControllerVisualZoneShape.RoundedRectangle, 46.15, 21.29, 8.6, 1.95),
        new(EmulationControllerVisualControl.FretOrange, ControllerVisualZoneShape.RoundedRectangle, 46.15, 24.22, 8.6, 1.95),
        new(EmulationControllerVisualControl.StrumUp, ControllerVisualZoneShape.RoundedRectangle, 46.15, 63.3, 5.9, 5.2),
        new(EmulationControllerVisualControl.StrumDown, ControllerVisualZoneShape.RoundedRectangle, 46.15, 68.5, 5.9, 5.2),
        new(EmulationControllerVisualControl.Whammy, ControllerVisualZoneShape.RoundedRectangle, 64.7, 62.7, 5.4, 21.1),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 75.6, 73, 7.7, 3.3),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 82.8, 76.2, 7.7, 3.3)
    ]);
}
