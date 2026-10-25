using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecTurboGrafxTurboStick() => new(ControllerArtworkFileNames.NecTurbografxTurbostick,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 16.0d, 38.5d, 24.0d, 33.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 60.7d, 58.4d, 10.1d, 13.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.4d, 46.2d, 10.7d, 13.9d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 48.5d, 52.8d, 5.0d, 5.6d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 55.6d, 43.2d, 5.0d, 5.6d)
            ]);
}
