using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXboxControllerS() => new(ControllerArtworkFileNames.XboxControllerS,
            [
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 22.9d, 27.0d, 11.0d, 15.0d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 59.1d, 51.7d, 11.0d, 15.0d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 29.8d, 48.8d, 13.0d, 17.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 67.1d, 38.9d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.3d, 30.2d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 62.0d, 30.2d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 67.2d, 21.4d, 5.4d, 8.4d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 75.8d, 45.3d, 4.2d, 6.2d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 78.9d, 38.0d, 4.2d, 6.2d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 42.2d, 45.0d, 4.5d, 5.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 53.3d, 45.0d, 4.5d, 5.5d)
            ]);
}
