using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyAsciiSphere360() => new(ControllerArtworkFileNames.SonyAsciiSphere360,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.9, 42.2, 21, 19.7),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.9, 42.2, 21, 19.7),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.9, 42.2, 21, 19.7),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.9, 42.2, 21, 19.7),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.5, 42.4, 8.4, 7.8),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 82.6, 35.5, 8.4, 7.8),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 67.3, 34.2, 8.4, 7.8),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 74.6, 27.5, 8.4, 7.8),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 60.8, 40.4, 8, 7.4),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 66.2, 49.2, 8, 7.4),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 44.9, 50.2, 4.2, 3.9),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 52.2, 50.2, 5.2, 3.9),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 46.541, 42.773, 2.096, 2.539),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 48.847, 42.773, 2.096, 2.539),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 51.153, 42.773, 2.096, 2.539),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 53.459, 42.773, 2.096, 2.539)
    ]);
}
