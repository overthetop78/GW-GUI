using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyFanatecSpeedster3() => new(ControllerArtworkFileNames.SonyFanatecSpeedster3,
    [
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 31.522, 24.609, 3.804, 2.734),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 35.870, 24.609, 3.804, 2.734),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 27.989, 28.711, 3.804, 2.734),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 32.065, 29.102, 3.804, 2.734),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 59.511, 24.609, 3.804, 2.734),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 64.402, 24.609, 3.804, 2.734),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 63.587, 29.102, 3.804, 2.734),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.478, 28.906, 3.804, 2.734),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 42.120, 28.711, 5.707, 3.906),
        new(EmulationControllerVisualControl.Menu, ControllerVisualZoneShape.Ellipse, 52.174, 28.711, 5.707, 3.906),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 42.120, 35.938, 5.707, 3.906),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 52.174, 35.938, 5.707, 3.906),
        new(EmulationControllerVisualControl.Buzzer, ControllerVisualZoneShape.Ellipse, 45.652, 31.250, 8.967, 6.641),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.Ellipse, 27.717, 63.672, 16.304, 6.641),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.Ellipse, 62.228, 66.211, 15.761, 7.422),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.RoundedRectangle, 8.696, 32.422, 4.891, 7.422),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 86.685, 32.422, 4.891, 7.422)
    ]);
}
