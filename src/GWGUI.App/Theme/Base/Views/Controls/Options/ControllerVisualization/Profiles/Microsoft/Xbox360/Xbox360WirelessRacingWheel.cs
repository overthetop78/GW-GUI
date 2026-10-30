using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXbox360WirelessRacingWheel() => new(ControllerArtworkFileNames.Xbox360WirelessRacingWheel,
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 46.0d, 43.0d, 8.4d, 10.7d),
                new(EmulationControllerVisualControl.Steering, ControllerVisualZoneShape.Ellipse, 10.9d, 1.1d, 78.9d, 86.9d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 25.7d, 42.1d, 8.3d, 10.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.0d, 47.6d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.0d, 43.5d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 66.1d, 43.5d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 69.0d, 39.2d, 3.5d, 5.7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 35.0d, 45.8d, 3.5d, 5.2d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 61.1d, 45.8d, 3.5d, 5.2d)
            ]);
}
