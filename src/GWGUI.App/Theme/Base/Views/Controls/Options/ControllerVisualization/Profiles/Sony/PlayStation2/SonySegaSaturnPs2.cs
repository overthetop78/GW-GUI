using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonySegaSaturnPs2() => new(ControllerArtworkFileNames.SonySegaSaturnPs2,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 15.82, 32.39, 16.41, 29.58),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 15.82, 32.39, 16.41, 29.58),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 15.82, 32.39, 16.41, 29.58),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 15.82, 32.39, 16.41, 29.58),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 63.48, 59.15, 7.81, 14.08),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 74.02, 48.94, 7.42, 13.38),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 83.98, 40.85, 7.81, 14.08),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 61.72, 39.08, 7.03, 12.68),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 70.12, 29.93, 6.25, 11.27),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 79.49, 23.24, 6.25, 11.27),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 41.6, 55.63, 5.86, 4.93),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.56, 55.63, 6.64, 4.93)
    ]);
}
