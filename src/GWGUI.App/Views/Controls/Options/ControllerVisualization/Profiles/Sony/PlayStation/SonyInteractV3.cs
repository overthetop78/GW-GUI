using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyInteractV3() => new(ControllerArtworkFileNames.SonyInteractV3,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 66.667, 24.609, 4.088, 2.539),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.013, 21.484, 4.088, 2.539),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 61.950, 21.484, 4.088, 2.539),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 66.667, 18.359, 4.088, 2.539),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 18.239, 14.844, 5.975, 3.516),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 14.780, 18.359, 5.975, 3.516),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 75.786, 14.844, 5.975, 3.516),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 79.560, 18.359, 5.975, 3.516),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 46.855, 18.945, 6.604, 1.562),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 46.855, 23.047, 6.604, 1.562),
        new(EmulationControllerVisualControl.Program, ControllerVisualZoneShape.Ellipse, 45.912, 29.883, 8.491, 2.148),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.Ellipse, 26.730, 65.039, 14.151, 14.648),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.Ellipse, 59.434, 65.039, 14.151, 14.648),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 29.167, 19.531, 2.673, 1.660),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 29.167, 24.512, 2.673, 1.660),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 25.157, 22.021, 2.673, 1.660),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 33.176, 22.021, 2.673, 1.660),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.RoundedRectangle, 2.516, 29.688, 5.660, 7.422),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 92.138, 29.688, 5.660, 7.422)
    ]);
}
