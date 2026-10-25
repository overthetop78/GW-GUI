using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAtariJaguarProController() => new(ControllerArtworkFileNames.AtariJaguarProController,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 27.6d, 38.9d, 11.8d, 13.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 27.6d, 38.9d, 11.8d, 13.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 27.6d, 38.9d, 11.8d, 13.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 27.6d, 38.9d, 11.8d, 13.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.6d, 39.3d, 5.4d, 6.2d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 63.4d, 43.9d, 5.1d, 5.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 59.0d, 49.2d, 5.0d, 5.7d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 64.4d, 34.3d, 4.0d, 4.7d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 59.4d, 38.5d, 4.0d, 4.7d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 56.0d, 43.8d, 4.0d, 4.7d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 7.6d, 2.2d, 18.6d, 10.3d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 73.8d, 2.2d, 18.6d, 10.3d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.RoundedRectangle, 44.2d, 47.6d, 3.4d, 3.9d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 48.6d, 47.6d, 3.6d, 3.9d),
                new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 39.3d, 61.7d, 4.6d, 2.0d),
                new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 47.2d, 61.7d, 4.5d, 2.0d),
                new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 54.9d, 61.7d, 4.6d, 2.0d),
                new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 39.3d, 67.1d, 4.6d, 2.0d),
                new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 47.2d, 67.1d, 4.5d, 2.0d),
                new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 54.9d, 67.1d, 4.6d, 2.0d),
                new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 39.3d, 72.7d, 4.6d, 2.1d),
                new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 47.2d, 72.7d, 4.5d, 2.1d),
                new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 54.9d, 72.7d, 4.6d, 2.1d),
                new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 39.3d, 78.3d, 4.6d, 2.0d),
                new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 47.2d, 78.3d, 4.5d, 2.0d),
                new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 54.9d, 78.3d, 4.6d, 2.0d)
            ]);
}
