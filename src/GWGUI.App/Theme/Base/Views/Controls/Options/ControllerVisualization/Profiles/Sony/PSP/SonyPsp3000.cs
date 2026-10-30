using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPsp3000() => new(ControllerArtworkFileNames.SonyPsp3000,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 3.30d, 30.30d, 15.50d, 31.10d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 3.30d, 30.30d, 15.50d, 31.10d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 3.30d, 30.30d, 15.50d, 31.10d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 3.30d, 30.30d, 15.50d, 31.10d),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 10.00d, 66.00d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 10.00d, 72.20d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 8.20d, 69.10d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 11.80d, 69.10d, 2.80d, 6.20d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 86.85d, 51.40d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 92.35d, 39.70d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 81.45d, 39.70d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 86.85d, 28.50d, 5.50d, 11.80d),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 5.50d, 8.30d, 18.00d, 7.50d),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 80.00d, 8.30d, 13.00d, 7.50d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 69.10d, 85.30d, 5.60d, 5.40d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 75.50d, 85.30d, 5.60d, 5.40d),
    ]);
}
