using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyFanatecSpeedster2() => new(ControllerArtworkFileNames.SonyFanatecSpeedster2,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 13.086, 32.302, 2.344, 4.124),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 12.695, 38.832, 2.344, 4.124),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 16.797, 32.990, 2.344, 4.124),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 16.406, 39.175, 2.344, 4.124),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 38.477, 32.990, 2.344, 4.124),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 41.992, 32.302, 2.344, 4.124),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 38.867, 38.832, 2.344, 4.124),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 42.773, 38.488, 2.344, 4.124),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 27.148, 60.137, 2.930, 2.749),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 27.148, 64.948, 2.930, 2.749),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 27.148, 69.416, 2.930, 2.749),
        new(EmulationControllerVisualControl.GearUp, ControllerVisualZoneShape.Ellipse, 55.469, 52.577, 4.688, 4.467),
        new(EmulationControllerVisualControl.GearDown, ControllerVisualZoneShape.Ellipse, 55.469, 57.388, 4.688, 4.467),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.Ellipse, 68.750, 50.859, 8.594, 13.058),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.Ellipse, 83.789, 50.859, 8.594, 13.058),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 13.867, 54.983, 1.172, 2.062),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 13.867, 61.168, 1.172, 2.062),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 12.109, 58.076, 1.172, 2.062),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 15.625, 58.076, 1.172, 2.062),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.RoundedRectangle, 3.906, 47.079, 3.516, 13.058),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 51.172, 47.079, 3.516, 13.058)
    ]);
}
