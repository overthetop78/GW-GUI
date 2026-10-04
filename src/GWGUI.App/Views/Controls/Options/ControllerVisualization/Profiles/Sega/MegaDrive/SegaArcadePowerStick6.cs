using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSegaArcadePowerStick6() => new(ControllerArtworkFileNames.SegaArcadePowerStick6,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 22d, 28d, 24d, 34d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 22d, 28d, 24d, 34d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 22d, 28d, 24d, 34d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 22d, 28d, 24d, 34d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 58d, 56d, 10d, 15d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 69d, 51d, 10d, 15d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 80d, 51d, 10d, 15d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 58d, 39d, 10d, 15d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 69d, 34d, 10d, 15d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 80d, 34d, 10d, 15d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 49d, 17d, 9d, 7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 59d, 17d, 9d, 7d)
            ]);
}
