using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXboxDuke() => new(ControllerArtworkFileNames.XboxDuke,
            [
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 14.8d, 29.2d, 13.4d, 17.5d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 62.3d, 53.6d, 12.4d, 18.0d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 22.7d, 50.7d, 14.3d, 17.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 73.6d, 44.4d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 79.3d, 35.1d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 67.7d, 35.1d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 73.5d, 25.8d, 5.8d, 8.4d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 83.9d, 44.5d, 5.6d, 8.3d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 83.9d, 27.1d, 5.6d, 8.3d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 41.2d, 64.0d, 4.6d, 5.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 52.8d, 64.0d, 4.6d, 5.5d)
            ]);
}
