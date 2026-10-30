using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSaturn3D() => new(ControllerArtworkFileNames.Saturn3dControlPad,
            [
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 12d, 25d, 25d, 23d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 18d, 51d, 19d, 18d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 18d, 51d, 19d, 18d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 18d, 51d, 19d, 18d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 18d, 51d, 19d, 18d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 54d, 55d, 9d, 9d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 65d, 52d, 9d, 9d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 77d, 50d, 9d, 9d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 53d, 44d, 9d, 9d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 64d, 41d, 9d, 9d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 75d, 39d, 9d, 9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 43d, 65d, 9d, 9d)
            ]);
}
