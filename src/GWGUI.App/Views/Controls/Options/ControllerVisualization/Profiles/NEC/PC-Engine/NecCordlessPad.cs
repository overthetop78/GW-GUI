using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecCordlessPad() => new(ControllerArtworkFileNames.NecCordlessPad,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.4d, 51.7d, 17.5d, 24.2d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.8d, 61.9d, 9.5d, 13.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.8d, 61.9d, 9.5d, 13.4d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.4d, 65.2d, 9.1d, 6.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 65.2d, 9.1d, 6.3d)
            ]);
}
