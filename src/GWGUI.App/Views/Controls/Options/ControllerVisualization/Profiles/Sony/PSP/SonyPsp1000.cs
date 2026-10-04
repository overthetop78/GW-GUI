using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPsp1000() => new(ControllerArtworkFileNames.SonyPsp1000,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 6.0d, 20.0d, 15.0d, 27.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 6.0d, 20.0d, 15.0d, 27.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 6.0d, 20.0d, 15.0d, 27.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 6.0d, 20.0d, 15.0d, 27.0d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 9.1d, 47.8d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 9.1d, 56.4d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 6.5d, 52.1d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 11.7d, 52.1d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 81.4d, 60.5d, 4.2d, 6.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 88.3d, 51.0d, 4.2d, 6.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 77.0d, 47.5d, 4.2d, 6.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 84.0d, 38.5d, 4.2d, 6.0d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 8.0d, 7.0d, 12.0d, 7.0d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 80.0d, 7.0d, 12.0d, 7.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 62.2d, 80.5d, 3.5d, 5.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 68.7d, 83.0d, 3.5d, 5.0d)
            ]);
}
