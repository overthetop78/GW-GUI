using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualShock3() => new(ControllerArtworkFileNames.SonyDualShock3,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 11.0d, 11.5d, 21.5d, 22.0d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 11.0d, 11.5d, 21.5d, 22.0d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 11.0d, 11.5d, 21.5d, 22.0d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 11.0d, 11.5d, 21.5d, 22.0d),
            new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 22.0d, 27.0d, 19.5d, 23.0d),
            new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 51.0d, 31.0d, 17.0d, 22.5d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 72.0d, 33.0d, 8.0d, 9.5d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.0d, 25.0d, 8.0d, 9.5d),
            new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 66.5d, 23.5d, 8.0d, 9.5d),
            new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 77.0d, 15.0d, 8.0d, 9.5d),
            new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 22.5d, 7.5d, 10.0d, 5.0d),
            new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 82.0d, 11.0d, 10.0d, 5.0d),
            new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 26.0d, 1.0d, 11.0d, 5.5d),
            new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 85.0d, 7.0d, 10.0d, 4.0d),
            new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.0d, 24.0d, 6.0d, 5.0d),
            new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 57.0d, 26.0d, 6.0d, 5.5d)
        ]);
}
