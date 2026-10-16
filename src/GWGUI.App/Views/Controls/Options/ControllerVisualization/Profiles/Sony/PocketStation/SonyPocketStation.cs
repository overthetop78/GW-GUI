using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPocketStation() => new(ControllerArtworkFileNames.SonyPocketStation,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 28, 57, 12, 9),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 26, 72, 12, 9),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 13, 64, 12, 9),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 42, 66, 12, 9),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69, 61, 18, 12)
        ]);
}
