using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonySeamic() => new(ControllerArtworkFileNames.SonySeamic,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.96, 36.48, 18.55, 22.57),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.96, 36.48, 18.55, 22.57),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.96, 36.48, 18.55, 22.57),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.96, 36.48, 18.55, 22.57),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.JoystickDirection, 24.6, 61.4, 13.5, 19.2),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.JoystickDirection, 24.6, 61.4, 13.5, 19.2),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.JoystickDirection, 24.6, 61.4, 13.5, 19.2),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.JoystickDirection, 24.6, 61.4, 13.5, 19.2),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 68.95, 61.15, 6.64, 8.92),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 76.95, 53.28, 6.64, 8.92),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 85.35, 46.72, 6.64, 8.92),
        new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 62.7, 46.46, 6.64, 8.92),
        new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 70.51, 39.11, 6.64, 8.92),
        new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 79.1, 35.43, 6.64, 8.92),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 41.8, 53.8, 6.4, 3.15),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 52.5, 53.8, 6.7, 3.15),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.RoundedRectangle, 48.4, 62.7, 4.3, 3.4)
    ]);
}
