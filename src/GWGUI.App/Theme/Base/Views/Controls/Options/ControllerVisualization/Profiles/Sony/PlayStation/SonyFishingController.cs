using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyFishingController() => new(ControllerArtworkFileNames.SonyFishingController,
    [
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 24.82, 14.45, 4.38, 1.95),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 37.23, 14.45, 4.74, 1.95),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 14.23, 24.41, 4.38, 2.34),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 10.58, 27.73, 4.38, 2.34),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 47.08, 24.41, 4.38, 2.34),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 50.36, 27.73, 4.38, 2.34),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 7.30, 33.79, 7.30, 3.91),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 15.33, 38.87, 7.30, 3.91),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 50.36, 33.79, 7.30, 3.91),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 42.34, 38.87, 7.30, 3.91),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.JoystickDirection, 26.28, 26.95, 16.79, 8.98),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.JoystickDirection, 26.28, 26.95, 16.79, 8.98),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.JoystickDirection, 26.28, 26.95, 16.79, 8.98),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.JoystickDirection, 26.28, 26.95, 16.79, 8.98),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 33.21, 30.66, 2.92, 1.56),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 70.07, 5.08, 28.47, 9.38),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 71.90, 32.81, 26.64, 8.79)
    ]);
}
