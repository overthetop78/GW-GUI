using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSegaSaturnVirtuaStickHss0136() => new(ControllerArtworkFileNames.SegaSaturnVirtuaStickHss0136,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 21.9d, 23.5d, 11.9d, 18.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 21.9d, 23.5d, 11.9d, 18.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 21.9d, 23.5d, 11.9d, 18.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 21.9d, 23.5d, 11.9d, 18.4d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 46.3d, 19.5d, 9.0d, 12.5d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 55.4d, 15.3d, 9.0d, 12.4d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 65.2d, 15.1d, 8.9d, 12.2d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 46.1d, 33.8d, 9.3d, 13.1d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 55.6d, 28.5d, 9.6d, 13.1d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 65.9d, 28.5d, 9.3d, 12.9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 20.4d, 5.2d, 3.7d, 4.2d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 43.7d, 48.8d, 10.4d, 14.5d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 75.9d, 33.5d, 10.0d, 13.5d)
            ]);
}
