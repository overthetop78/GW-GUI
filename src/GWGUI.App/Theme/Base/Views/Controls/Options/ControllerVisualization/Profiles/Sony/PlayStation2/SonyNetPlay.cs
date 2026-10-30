using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyNetPlay() => new(ControllerArtworkFileNames.SonyNetPlay,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 3.1, 35.7, 6.6, 18.4),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 3.1, 35.7, 6.6, 18.4),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 3.1, 35.7, 6.6, 18.4),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 3.1, 35.7, 6.6, 18.4),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.JoystickDirection, 10.8, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.JoystickDirection, 10.8, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.JoystickDirection, 10.8, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.JoystickDirection, 10.8, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.JoystickDirection, 83.1, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.JoystickDirection, 83.1, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.JoystickDirection, 83.1, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.JoystickDirection, 83.1, 48.6, 6.5, 18.4),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 12.7, 54, 2.7, 7.6),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 85, 54, 2.7, 7.6),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 92.8, 50.3, 2.1, 5.9),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 95.1, 43.5, 2.1, 5.9),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 89.7, 43.5, 2.1, 5.9),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 92.1, 36.2, 2.1, 5.9),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 85.2, 29.2, 1.9, 3.3),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 87.5, 35, 1.9, 3.3),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 13.5, 28.6, 1.5, 4.3),
        new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 11.1, 34.1, 1.5, 4.3)
    ]);
}
