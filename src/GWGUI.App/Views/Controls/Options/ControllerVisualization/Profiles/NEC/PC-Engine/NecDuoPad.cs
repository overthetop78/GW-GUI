using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecDuoPad() => new(ControllerArtworkFileNames.NecDuopad,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 6.6d, 51.7d, 17.6d, 33.2d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.6d, 65.4d, 8.0d, 16.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 81.4d, 65.4d, 8.0d, 16.7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.9d, 70.1d, 7.7d, 6.9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 70.1d, 7.7d, 6.9d)
            ]);
}
