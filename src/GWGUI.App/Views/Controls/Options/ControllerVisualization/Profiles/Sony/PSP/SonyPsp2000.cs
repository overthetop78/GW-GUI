using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPsp2000() => new(ControllerArtworkFileNames.SonyPsp2000,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 4.10d, 22.70d, 14.70d, 33.30d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 4.10d, 22.70d, 14.70d, 33.30d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 4.10d, 22.70d, 14.70d, 33.30d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 4.10d, 22.70d, 14.70d, 33.30d),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 9.70d, 63.60d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 9.70d, 69.80d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 7.90d, 66.70d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 11.50d, 66.70d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 86.85d, 47.90d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 92.15d, 35.90d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 81.45d, 35.90d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 86.65d, 23.00d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 8.00d, 5.00d, 9.00d, 5.00d),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 83.00d, 5.00d, 9.00d, 5.00d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 69.30d, 84.40d, 5.60d, 5.40d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 75.70d, 84.40d, 5.60d, 5.40d),
    ]);
}
