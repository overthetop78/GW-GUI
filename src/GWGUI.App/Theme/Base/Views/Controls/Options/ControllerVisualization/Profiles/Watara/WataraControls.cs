using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;
internal static partial class ControllerArtworkCatalog
{
    // Photo: Letzte*Spieler; transparent background by Alexis Jazz. Cropped and resized for integrated controls.
    // https://commons.wikimedia.org/wiki/File:Watara_Supervision_game_console.png ; CC BY-SA 3.0 https://creativecommons.org/licenses/by-sa/3.0/
    private static ProfileDefinition CreateWataraSupervisionControls() => new(
        ControllerArtworkFileNames.WataraSupervisionControls,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 24, 14, 10, 12),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 24, 37, 11, 13),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 14, 24, 12, 12),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 32.5, 24, 12, 12),
            new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 72, 20, 14, 16),
            new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 53, 31, 14, 16),
            new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 47, 8, 12, 5),
            new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 66, 9, 12, 5)
        ]
    );
}