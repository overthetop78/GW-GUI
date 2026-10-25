using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateColecoRollerController() => new(ControllerArtworkFileNames.ColecoRollerController,
    [
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 46, 20.5, 5, 8),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 54, 20.5, 5, 8),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 50, 13.5, 5, 7),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 50, 28, 5, 7),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 35.5, 60.5, 5.5, 10),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 41.8, 71.8, 5.5, 10),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 63.5, 60.5, 5.5, 10),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.RoundedRectangle, 57.4, 71.8, 5.5, 10)
    ]);
}
