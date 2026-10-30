using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXbox360WirelessSpeedWheel() => new(ControllerArtworkFileNames.Xbox360WirelessSpeedWheel,
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 46.4d, 62.6d, 7.4d, 10.9d),
                new(EmulationControllerVisualControl.Steering, ControllerVisualZoneShape.Ellipse, 4.2d, 2.4d, 91.5d, 94.5d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.9d, 24.1d, 10.7d, 16.1d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 82.5d, 34.0d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 86.9d, 27.6d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 78.0d, 27.6d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 82.5d, 21.1d, 4.9d, 7.9d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 41.9d, 65.8d, 3.1d, 5.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 54.8d, 65.8d, 3.1d, 5.0d)
            ]);
}
