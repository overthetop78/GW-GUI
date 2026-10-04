using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreatePowerplayCruiser() => new(ControllerArtworkFileNames.PowerplayCruiser,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 29.8d, 10.6d, 41.4d, 39.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 14.8d, 64.9d, 19.2d, 17.8d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 67.8d, 65.0d, 18.9d, 17.8d)
            ]);
}
