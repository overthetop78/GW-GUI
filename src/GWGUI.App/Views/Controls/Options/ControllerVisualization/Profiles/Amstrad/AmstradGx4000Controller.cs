using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAmstradGx4000Controller() => new(ControllerArtworkFileNames.AmstradGx4000Controller,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 16.8d, 51.0d, 19.1d, 26.8d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 16.8d, 51.0d, 19.1d, 26.8d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 16.8d, 51.0d, 19.1d, 26.8d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 16.8d, 51.0d, 19.1d, 26.8d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 66.0d, 40.5d, 8.5d, 13.5d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 77.0d, 34.7d, 8.5d, 13.5d)
        ]);
}
