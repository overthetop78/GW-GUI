using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    // Panasonic FZ-JP1 photographed by Evan-Amos. Cropped and background removed; CC BY-SA 3.0.
    // https://commons.wikimedia.org/wiki/File:3DO-FZ1-Console-Set.png ; https://creativecommons.org/licenses/by-sa/3.0/
    // L/R are on the rear edge and are not drawn as visible buttons on this front photograph.
    private static ProfileDefinition CreatePanasonicFzJp1() => new(
        ControllerArtworkFileNames.PanasonicFzJp1,
        [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 14.06, 41.36, 17.58, 28.81),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 14.06, 41.36, 17.58, 28.81),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 14.06, 41.36, 17.58, 28.81),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 14.06, 41.36, 17.58, 28.81),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 67.97, 50.32, 7.89, 13.69),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 74.65, 38.47, 7.89, 13.69),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 82.53, 28.21, 7.89, 13.69),
        new(EmulationControllerVisualControl.Stop, ControllerVisualZoneShape.RoundedRectangle, 39.16, 50.71, 6.67, 5.53),
        new(EmulationControllerVisualControl.PlayPause, ControllerVisualZoneShape.RoundedRectangle, 53.72, 47.82, 6.67, 5.53)
    ]
    );
}
