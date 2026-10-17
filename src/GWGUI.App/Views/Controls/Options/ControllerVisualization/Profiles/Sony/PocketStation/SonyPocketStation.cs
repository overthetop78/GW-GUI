using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPocketStation() => new(ControllerArtworkFileNames.SonyPocketStation,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 26.95, 54.88, 12, 9),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 25.21, 69.33, 12, 9),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 12.50, 61.13, 12, 9),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 40.24, 63.08, 12, 9),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 65.28, 59.43, 18, 12)
        ]);
}
