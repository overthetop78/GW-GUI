using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPsp1000() => new(ControllerArtworkFileNames.SonyPsp1000,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 2.3d, 24.1d, 15.1d, 38.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 2.3d, 24.1d, 15.1d, 38.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 2.3d, 24.1d, 15.1d, 38.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 2.3d, 24.1d, 15.1d, 38.4d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 8.0d, 65.2d, 2.6d, 6.3d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 8.0d, 75.0d, 2.6d, 6.3d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 5.5d, 70.1d, 2.6d, 6.3d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 10.5d, 70.1d, 2.6d, 6.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 86.3d, 47.3d, 5.9d, 13.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 91.6d, 34.4d, 5.9d, 13.4d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 80.3d, 34.4d, 5.9d, 13.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 85.9d, 21.4d, 5.9d, 13.4d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 8.6d, 3.6d, 7.0d, 5.8d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 84.4d, 3.6d, 7.0d, 5.8d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 67.8d, 85.3d, 7.0d, 8.9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 74.2d, 85.3d, 7.8d, 8.9d)
            ]);
}
