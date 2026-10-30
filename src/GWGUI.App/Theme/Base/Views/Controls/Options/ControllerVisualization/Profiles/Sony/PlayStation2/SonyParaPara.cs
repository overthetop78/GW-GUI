using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyParaPara() => new(ControllerArtworkFileNames.SonyParaPara,
    [
        new(EmulationControllerVisualControl.OpticalSensor1, ControllerVisualZoneShape.Ellipse, 8.984, 3.933, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor1, ControllerVisualZoneShape.Ellipse, 1.758, 38.202, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor1, ControllerVisualZoneShape.Ellipse, 15.820, 38.202, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor2, ControllerVisualZoneShape.Ellipse, 28.711, 20.787, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor2, ControllerVisualZoneShape.Ellipse, 21.484, 54.494, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor2, ControllerVisualZoneShape.Ellipse, 35.547, 54.494, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor3, ControllerVisualZoneShape.Ellipse, 48.242, 34.831, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor3, ControllerVisualZoneShape.Ellipse, 41.016, 73.596, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor3, ControllerVisualZoneShape.Ellipse, 55.078, 73.596, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor4, ControllerVisualZoneShape.Ellipse, 67.969, 20.787, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor4, ControllerVisualZoneShape.Ellipse, 60.742, 54.494, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor4, ControllerVisualZoneShape.Ellipse, 74.805, 54.494, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor5, ControllerVisualZoneShape.Ellipse, 87.500, 3.933, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor5, ControllerVisualZoneShape.Ellipse, 80.273, 38.202, 3.516, 10.112),
        new(EmulationControllerVisualControl.OpticalSensor5, ControllerVisualZoneShape.Ellipse, 94.336, 38.202, 3.516, 10.112),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 46.680, 55.618, 2.930, 8.427),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 50.391, 56.180, 2.930, 8.427),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 46.875, 66.854, 2.930, 8.427),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 50.586, 66.854, 2.930, 8.427)
    ]);
}
