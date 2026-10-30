using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyAnalogJoystick() => new(ControllerArtworkFileNames.SonyAnalogJoystick,
    [
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 12.5d, 11.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 12.5d, 23.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 7.5d, 17.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 17.5d, 17.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.Ellipse, 82.5d, 11.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.Ellipse, 82.5d, 23.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.Ellipse, 77.5d, 17.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.Ellipse, 87.5d, 17.5d, 5.0d, 5.0d),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 7.0d, 10.0d, 17.0d, 16.0d),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 76.0d, 10.0d, 17.0d, 16.0d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 44.0d, 58.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 51.0d, 58.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 44.0d, 51.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 51.0d, 51.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 37.0d, 52.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 59.0d, 52.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 37.0d, 59.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 59.0d, 59.0d, 7.0d, 7.0d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 27.0d, 49.0d, 4.0d, 3.0d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 32.0d, 49.0d, 4.0d, 3.0d),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.RoundedRectangle, 69.0d, 49.0d, 4.0d, 3.0d)
    ]);
}
