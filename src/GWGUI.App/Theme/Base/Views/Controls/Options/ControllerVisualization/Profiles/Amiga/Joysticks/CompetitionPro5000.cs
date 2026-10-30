using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateCompetitionPro5000() => new(ControllerArtworkFileNames.CompetitionPro5000,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 22.5d, 38.0d, 54.7d, 39.1d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 5.8d, 3.3d, 30.9d, 23.1d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 63.9d, 3.3d, 31.0d, 23.3d)
            ]);
}
