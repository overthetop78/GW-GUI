using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualAnalog() => new(ControllerArtworkFileNames.SonyDualAnalog,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.0d, 20.0d, 25.0d, 28.0d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.0d, 20.0d, 25.0d, 28.0d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.0d, 20.0d, 25.0d, 28.0d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.0d, 20.0d, 25.0d, 28.0d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 34.5d, 46.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 34.5d, 58.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 29.5d, 52.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 39.5d, 52.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.Ellipse, 60.5d, 46.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.Ellipse, 60.5d, 58.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.Ellipse, 55.5d, 52.5d, 5.0d, 5.0d),
                new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.Ellipse, 65.5d, 52.5d, 5.0d, 5.0d),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 29.0d, 45.0d, 16.0d, 19.0d),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 55.0d, 45.0d, 16.0d, 19.0d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.0d, 38.0d, 9.0d, 10.0d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.0d, 27.0d, 9.0d, 11.0d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 67.0d, 27.0d, 9.0d, 11.0d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75.0d, 17.0d, 9.0d, 11.0d),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 11.0d, 7.0d, 17.0d, 11.0d),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 71.0d, 7.0d, 17.0d, 11.0d),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 13.0d, 1.0d, 15.0d, 6.0d),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 72.0d, 1.0d, 15.0d, 6.0d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.5d, 31.0d, 6.0d, 5.0d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 57.0d, 31.0d, 6.0d, 5.0d),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.RoundedRectangle, 49.0d, 44.0d, 5.0d, 4.0d)
    ]);
}
