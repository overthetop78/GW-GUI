using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyOnimushaKatana() => new(ControllerArtworkFileNames.SonyOnimushaKatana,
    [
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 18.62, 27.73, 4.45, 2.34),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.RoundedRectangle, 25.51, 27.73, 4.45, 2.34),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 18.62, 31.45, 4.45, 2.34),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 25.51, 31.45, 4.45, 2.34),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 23.08, 25.20, 2.43, 1.17),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 21.86, 36.33, 4.86, 0.98),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 21.86, 39.65, 4.86, 0.98),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.JoystickDirection, 19.43, 52.93, 9.72, 4.69),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.JoystickDirection, 19.43, 52.93, 9.72, 4.69),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.JoystickDirection, 19.43, 52.93, 9.72, 4.69),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.JoystickDirection, 19.43, 52.93, 9.72, 4.69),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 22.87, 54.59, 2.83, 1.37),
        new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.JoystickDirection, 19.43, 72.85, 9.72, 4.69),
        new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.JoystickDirection, 19.43, 72.85, 9.72, 4.69),
        new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.JoystickDirection, 19.43, 72.85, 9.72, 4.69),
        new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.JoystickDirection, 19.43, 72.85, 9.72, 4.69),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 22.87, 74.51, 2.83, 1.37),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 17.41, 61.72, 13.77, 6.64),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 17.41, 61.72, 13.77, 6.64),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 17.41, 61.72, 13.77, 6.64),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 17.41, 61.72, 13.77, 6.64),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 70.45, 30.27, 10.53, 4.10),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 70.45, 38.48, 10.53, 4.10),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 70.45, 69.34, 10.53, 4.10),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 70.45, 77.73, 10.53, 4.10)
    ]);
}
