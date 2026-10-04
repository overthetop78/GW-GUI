using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSuzoTheArcadeTurbo() => new(ControllerArtworkFileNames.SuzoTheArcadeTurbo,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 34.4d, 20.4d, 30.8d, 32.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 43.1d, 30.0d, 13.2d, 13.4d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 39.9d, 81.2d, 20.0d, 9.3d)
            ]);
}
