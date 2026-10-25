using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyAsciiGrip() => new(ControllerArtworkFileNames.SonyAsciiGrip,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 28.3, 15.2, 44.2, 22.7),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 28.3, 15.2, 44.2, 22.7),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 28.3, 15.2, 44.2, 22.7),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 28.3, 15.2, 44.2, 22.7),
        new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.RoundedRectangle, 7, 10.5, 20.9, 13.7),
        new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.RoundedRectangle, 71.3, 10.5, 20.9, 13.7),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.RoundedRectangle, 6.6, 28.9, 22.5, 12.1),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.RoundedRectangle, 71.3, 28.9, 22.5, 12.1),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 20.9, 45.3, 20.5, 6.6),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 59.7, 45.3, 20.5, 6.6),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 25.6, 56.1, 19.4, 6.6),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 55.8, 56.1, 19.4, 6.6),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 26.7, 4.5, 19.8, 7.4),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 54.3, 4.5, 19.8, 7.4)
    ]);
}
