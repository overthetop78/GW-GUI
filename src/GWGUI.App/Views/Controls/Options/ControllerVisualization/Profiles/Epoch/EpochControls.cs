using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateEpochSuperCassetteVisionController() => new(
        ControllerArtworkFileNames.EpochSuperCassetteVisionController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 40, 35, 12, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 36, 59, 12, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 25, 45, 12, 10),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 51, 47, 12, 10),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 37, 2, 12, 15),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 88, 20, 10, 16)
    ]);

}

