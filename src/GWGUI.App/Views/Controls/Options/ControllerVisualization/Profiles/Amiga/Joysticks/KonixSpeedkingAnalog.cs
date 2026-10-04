using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateKonixSpeedkingAnalog() => new(ControllerArtworkFileNames.KonixSpeedkingAnalog,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 25.0d, 17.1d, 50.0d, 53.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 12.9d, 74.0d, 14.3d, 15.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 73.4d, 74.0d, 14.0d, 15.0d)
            ]);
}
