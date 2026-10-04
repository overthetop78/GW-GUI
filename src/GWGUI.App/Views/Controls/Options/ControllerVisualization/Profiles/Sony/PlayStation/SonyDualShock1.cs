using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualShock1() => new(ControllerArtworkFileNames.SonyDualshock1,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 21.5d, 10.5d, 16.0d, 18.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 21.5d, 10.5d, 16.0d, 18.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 21.5d, 10.5d, 16.0d, 18.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 21.5d, 10.5d, 16.0d, 18.5d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 28.0d, 27.0d, 13.0d, 15.5d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 53.6d, 34.5d, 13.0d, 15.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 73.7d, 39.5d, 6.7d, 8.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 84.3d, 34.8d, 6.4d, 8.5d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 68.6d, 30.6d, 6.1d, 8.2d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 79.5d, 27.0d, 6.1d, 7.8d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 30.0d, 3.0d, 14.0d, 10.0d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 78.0d, 18.0d, 15.0d, 8.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 44.6d, 25.3d, 4.7d, 4.6d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 59.7d, 29.9d, 5.0d, 4.3d)
            ]);
}
