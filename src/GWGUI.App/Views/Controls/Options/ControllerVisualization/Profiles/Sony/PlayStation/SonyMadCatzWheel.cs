using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyMadCatzWheel() => new(ControllerArtworkFileNames.SonyMadCatzWheel,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 17.092, 15.234, 4.322, 4.297),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 22.004, 18.555, 4.322, 4.297),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 26.326, 22.461, 4.322, 4.297),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.495, 15.430, 4.322, 4.297),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 67.387, 18.555, 4.322, 4.297),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 63.458, 22.461, 4.322, 4.297),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 43.222, 49.219, 3.536, 2.930),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 47.151, 49.219, 3.536, 2.930),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 45.187, 28.516, 4.322, 1.758),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.Ellipse, 26.130, 62.891, 12.770, 15.820),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.Ellipse, 53.635, 62.891, 12.770, 15.820),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 27.112, 39.648, 1.965, 1.953),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 27.112, 45.508, 1.965, 1.953),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 24.165, 42.578, 1.965, 1.953),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 30.059, 42.578, 1.965, 1.953),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.RoundedRectangle, 3.340, 38.086, 3.536, 7.422),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 88.016, 38.086, 3.536, 7.422),
        new(EmulationControllerVisualControl.GearUp, ControllerVisualZoneShape.Ellipse, 93.320, 43.164, 4.715, 2.539),
        new(EmulationControllerVisualControl.GearDown, ControllerVisualZoneShape.Ellipse, 93.320, 45.898, 4.715, 2.539)
    ]);
}
