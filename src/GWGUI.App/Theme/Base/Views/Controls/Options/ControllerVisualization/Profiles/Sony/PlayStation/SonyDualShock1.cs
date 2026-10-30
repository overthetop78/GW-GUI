using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualShock1() => new(ControllerArtworkFileNames.SonyDualshock1,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7d, 50d, 22d, 19d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7d, 50d, 22d, 19d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7d, 50d, 22d, 19d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7d, 50d, 22d, 19d),
            new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 30.5d, 71d, 4d, 4d),
            new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 30.5d, 80d, 4d, 4d),
            new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 26d, 75.5d, 4d, 4d),
            new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 35d, 75.5d, 4d, 4d),
            new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.Ellipse, 64d, 73d, 4d, 4d),
            new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.Ellipse, 64d, 82d, 4d, 4d),
            new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.Ellipse, 59.5d, 77.5d, 4d, 4d),
            new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.Ellipse, 68.5d, 77.5d, 4d, 4d),
            new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 24d, 69.5d, 18d, 18d),
            new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 57d, 71.5d, 18d, 18d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 78d, 65.5d, 8d, 8.5d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 87d, 57.5d, 8d, 8.5d),
            new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 69d, 57d, 8d, 8.5d),
            new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 78d, 49d, 8d, 8.5d),
            // The four upper buttons are shown in the separate photographic detail.
            new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 12d, 3d, 14d, 7d),
            new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 75d, 2d, 14d, 8d),
            new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 13d, 14.5d, 14d, 11.5d),
            new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 75d, 14d, 14d, 12d),
            new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 38d, 59d, 6d, 4d),
            new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 57d, 59d, 7d, 4d),
            new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.RoundedRectangle, 47d, 68.5d, 6d, 4d)
        ]);
}
