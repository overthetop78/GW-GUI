using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPspGo() => new(ControllerArtworkFileNames.SonyPspGo,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 13.70d, 64.30d, 20.90d, 26.60d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 13.70d, 64.30d, 20.90d, 26.60d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 13.70d, 64.30d, 20.90d, 26.60d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 13.70d, 64.30d, 20.90d, 26.60d),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 40.60d, 76.90d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 40.60d, 83.10d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 38.80d, 80.00d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 42.40d, 80.00d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 72.65d, 81.75d, 5.50d, 7.50d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 78.55d, 74.55d, 5.50d, 7.50d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 66.95d, 74.55d, 5.50d, 7.50d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 72.65d, 66.75d, 5.50d, 7.50d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 55.40d, 76.10d, 5.60d, 5.40d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 55.40d, 82.50d, 5.60d, 5.40d),
    ]);
}
