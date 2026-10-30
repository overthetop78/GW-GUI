using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualShock2() => new(ControllerArtworkFileNames.Playstation2,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.4d, 18.6d, 19.0d, 33.6d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 30.7d, 48.1d, 14.4d, 24.9d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 55.0d, 48.1d, 14.4d, 24.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.5d, 40.1d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 82.5d, 30.0d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 68.6d, 30.0d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75.5d, 20.0d, 7.6d, 12.0d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 15.5d, 13.1d, 12.7d, 8.3d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 71.8d, 13.1d, 12.7d, 8.3d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 15.5d, 8.4d, 12.7d, 5.0d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 71.8d, 8.4d, 12.7d, 5.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.0d, 41.0d, 4.2d, 4.6d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 58.3d, 41.0d, 4.2d, 4.6d)
            ]);
}
