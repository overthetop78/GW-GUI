using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPsp1000() => new(ControllerArtworkFileNames.SonyPsp1000,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 4d, 28d, 14d, 29d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 4d, 28d, 14d, 29d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 4d, 28d, 14d, 29d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 4d, 28d, 14d, 29d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 8.5d, 62d, 2.6d, 6d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 8.5d, 70d, 2.6d, 6d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 6.5d, 66d, 2.6d, 6d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 10.5d, 66d, 2.6d, 6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 81.5d, 49d, 6d, 14d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 87.5d, 37d, 6d, 14d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 76d, 36d, 6d, 14d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 82d, 23d, 6d, 14d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 10.5d, 2.5d, 8.5d, 7d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 82.5d, 4d, 9d, 7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 63d, 82d, 7d, 8d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 69d, 82d, 7d, 8d)
            ]);
}
