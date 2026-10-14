using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateColecoRollerController() => new(ControllerArtworkFileNames.ColecoRollerController,
    [
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 40, 19, 6, 7),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 49, 19, 6, 7),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 44, 15, 6, 5),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 44, 26, 6, 5),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 33, 57, 6, 9),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 40.5, 65, 6, 9),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 61, 50, 6, 10),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.RoundedRectangle, 56, 60, 6, 9)
    ]);
}
