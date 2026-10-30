using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyRe4Chainsaw() => new(ControllerArtworkFileNames.SonyRe4Chainsaw,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 40.63, 63.09, 11.43, 7.03),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 40.63, 63.09, 11.43, 7.03),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 40.63, 63.09, 11.43, 7.03),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 40.63, 63.09, 11.43, 7.03),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.JoystickDirection, 40.95, 74.22, 9.52, 5.86),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.JoystickDirection, 40.95, 74.22, 9.52, 5.86),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.JoystickDirection, 40.95, 74.22, 9.52, 5.86),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.JoystickDirection, 40.95, 74.22, 9.52, 5.86),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 44.60, 76.46, 2.22, 1.37),
        new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.JoystickDirection, 52.38, 84.38, 9.52, 5.86),
        new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.JoystickDirection, 52.38, 84.38, 9.52, 5.86),
        new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.JoystickDirection, 52.38, 84.38, 9.52, 5.86),
        new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.JoystickDirection, 52.38, 84.38, 9.52, 5.86),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 56.03, 86.62, 2.22, 1.37),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 66.98, 78.12, 5.08, 3.12),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75.56, 78.12, 5.08, 3.12),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 66.98, 83.40, 5.08, 3.12),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 75.56, 83.40, 5.08, 3.12),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 71.75, 65.04, 5.08, 3.12),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 55.24, 58.79, 4.76, 1.95),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 62.86, 58.79, 5.40, 1.95)
    ]);
}
