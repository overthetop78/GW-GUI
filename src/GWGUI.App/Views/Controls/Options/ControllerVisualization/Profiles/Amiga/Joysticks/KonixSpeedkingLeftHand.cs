using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateKonixSpeedkingLeftHand() => new(ControllerArtworkFileNames.KonixSpeedkingLeftHand,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 30.7d, 12.0d, 41.0d, 23.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 81.0d, 43.0d, 13.0d, 10.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 77.0d, 56.0d, 13.0d, 10.0d)
            ]);
}
