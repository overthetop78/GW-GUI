using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyJogCon() => new(ControllerArtworkFileNames.SonyJogCon,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.5, 15.5, 17.5, 21),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.5, 15.5, 17.5, 21),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.5, 15.5, 17.5, 21),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.5, 15.5, 17.5, 21),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.5, 29.1, 8.2, 10.5),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.6, 20.5, 8.4, 10.5),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 67, 20.5, 8.4, 10.5),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75.2, 11.5, 8.4, 10.5),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 38.5, 14.3, 6.1, 4.5),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 54.5, 14.3, 6.8, 4.5),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 50.8, 8.8, 4.5, 5.8),
        new(EmulationControllerVisualControl.Steering, ControllerVisualZoneShape.Ellipse, 33.4, 24.5, 34.2, 42)
    ]);
}
