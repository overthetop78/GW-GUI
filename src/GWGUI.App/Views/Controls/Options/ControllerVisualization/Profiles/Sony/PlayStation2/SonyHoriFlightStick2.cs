using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyHoriFlightStick2() => new(ControllerArtworkFileNames.SonyHoriFlightStick2,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 67.383, 7.082, 2.344, 3.683),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 67.383, 12.465, 2.344, 3.683),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 65.234, 9.915, 2.539, 3.399),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 69.531, 9.915, 2.539, 3.399),
        new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.RoundedRectangle, 77.930, 4.249, 2.734, 2.266),
        new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.RoundedRectangle, 77.930, 10.482, 2.734, 2.266),
        new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.RoundedRectangle, 75.977, 6.516, 1.758, 3.966),
        new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.RoundedRectangle, 80.859, 6.516, 1.758, 3.966),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 78.125, 6.799, 2.539, 3.683),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 71.875, 16.147, 2.344, 3.683),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 79.492, 16.431, 2.539, 3.966),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 75.000, 19.830, 3.906, 5.949),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.RoundedRectangle, 75.391, 30.595, 4.297, 9.065),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.RoundedRectangle, 75.391, 47.025, 4.297, 9.065),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.RoundedRectangle, 72.656, 39.660, 3.711, 7.649),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.RoundedRectangle, 79.492, 39.660, 3.711, 7.649),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.RoundedRectangle, 23.828, 52.975, 2.930, 3.683),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.RoundedRectangle, 23.828, 59.773, 2.930, 3.683),
        new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.RoundedRectangle, 40.430, 11.615, 2.344, 1.983),
        new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.RoundedRectangle, 40.430, 16.431, 2.344, 1.983),
        new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.RoundedRectangle, 38.672, 13.598, 1.758, 3.399),
        new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.RoundedRectangle, 42.969, 13.598, 1.758, 3.399),
        new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.RoundedRectangle, 27.930, 22.096, 1.172, 2.833),
        new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.RoundedRectangle, 29.297, 22.096, 1.367, 2.833),
        new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.RoundedRectangle, 30.859, 22.096, 1.172, 2.833),
        new(EmulationControllerVisualControl.FaceD, ControllerVisualZoneShape.Ellipse, 32.227, 25.779, 4.492, 6.799),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 27.734, 40.227, 2.930, 3.966),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 24.414, 32.578, 1.367, 2.833),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 25.977, 32.011, 1.367, 2.833),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 27.539, 31.445, 1.367, 2.833),
        new(EmulationControllerVisualControl.DialLeft, ControllerVisualZoneShape.RoundedRectangle, 33.008, 38.527, 2.930, 6.799),
        new(EmulationControllerVisualControl.DialRight, ControllerVisualZoneShape.RoundedRectangle, 36.133, 38.527, 2.930, 6.799)
    ]);
}
