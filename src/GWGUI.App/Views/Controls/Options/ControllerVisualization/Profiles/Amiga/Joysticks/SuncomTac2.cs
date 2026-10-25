using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSuncomTac2() => new(ControllerArtworkFileNames.SuncomTac2,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 37.2d, 32.2d, 25.0d, 26.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 13.6d, 67.4d, 16.4d, 18.3d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 69.4d, 67.4d, 16.8d, 18.3d)
            ]);
}
