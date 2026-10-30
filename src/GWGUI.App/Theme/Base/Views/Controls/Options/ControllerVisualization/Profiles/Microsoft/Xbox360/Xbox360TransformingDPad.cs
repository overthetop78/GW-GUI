using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXbox360TransformingDPad() => new(ControllerArtworkFileNames.Xbox360TransformingDpad,
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 44.7d, 20.0d, 10.6d, 17.0d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 18.4d, 17.9d, 10.5d, 17.3d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 55.1d, 39.9d, 11.0d, 18.4d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 29.4d, 38.7d, 13.7d, 20.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 72.1d, 33.1d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 79.3d, 23.1d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 65.9d, 23.1d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 73.0d, 11.4d, 6.4d, 10.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 37.8d, 24.0d, 5.0d, 8.1d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 58.1d, 24.0d, 5.0d, 8.1d)
            ]);
}
