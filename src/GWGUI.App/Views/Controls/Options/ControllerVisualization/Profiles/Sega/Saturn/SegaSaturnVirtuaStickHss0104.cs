using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSegaSaturnVirtuaStickHss0104() => new(ControllerArtworkFileNames.SegaSaturnVirtuaStickHss0104,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 10.7d, 32.3d, 12.3d, 17.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 10.7d, 32.3d, 12.3d, 17.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 10.7d, 32.3d, 12.3d, 17.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 10.7d, 32.3d, 12.3d, 17.5d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 57.3d, 25.0d, 7.0d, 10.5d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 65.4d, 17.4d, 7.0d, 10.5d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 75.7d, 14.5d, 7.0d, 10.5d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 60.8d, 39.3d, 7.0d, 10.5d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 68.6d, 31.6d, 7.0d, 10.5d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 78.6d, 28.9d, 7.0d, 10.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 39.8d, 60.0d, 7.0d, 5.2d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 52.8d, 48.7d, 6.1d, 10.1d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 86.5d, 16.5d, 6.2d, 10.0d)
            ]);
}
