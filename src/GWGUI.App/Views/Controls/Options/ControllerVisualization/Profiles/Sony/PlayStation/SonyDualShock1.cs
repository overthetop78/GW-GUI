using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualShock1() => new(ControllerArtworkFileNames.SonyDualshock1,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.0d, 22.0d, 25.0d, 28.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.0d, 22.0d, 25.0d, 28.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.0d, 22.0d, 25.0d, 28.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.0d, 22.0d, 25.0d, 28.0d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 33d, 51.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 33d, 63.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 28d, 57.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 38d, 57.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.Ellipse, 61.5d, 51.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.Ellipse, 61.5d, 63.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.Ellipse, 56.5d, 57.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.Ellipse, 66.5d, 57.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 28.0d, 49.0d, 17.0d, 23.0d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 55.0d, 49.0d, 17.0d, 23.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 44.0d, 9.0d, 10.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 85.0d, 32.0d, 9.0d, 11.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 67.0d, 32.0d, 9.0d, 11.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 22.0d, 9.0d, 11.0d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 11.0d, 7.0d, 17.0d, 11.0d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 71.0d, 7.0d, 17.0d, 11.0d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 13.0d, 1.0d, 15.0d, 6.0d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 72.0d, 1.0d, 15.0d, 6.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 38.0d, 35.0d, 7.0d, 6.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 56.0d, 35.0d, 7.0d, 6.0d),
                new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.RoundedRectangle, 47.0d, 48.0d, 6.0d, 6.0d)
            ]);
}
