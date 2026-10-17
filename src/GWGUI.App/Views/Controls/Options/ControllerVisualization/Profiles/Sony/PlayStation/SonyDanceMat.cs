using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDanceMat() => new(ControllerArtworkFileNames.SonyDanceMat,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 35, 28.5, 30, 18),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 35, 79, 30, 19),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 5, 54, 23, 17),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 74, 54, 22, 17),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 5.5, 29, 23, 18),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 71, 29, 23, 18),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 71, 79, 23, 16),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.RoundedRectangle, 5.5, 79, 23, 16),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 13.5, 18.5, 10, 3),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 78, 18.5, 10, 3)
    ]);
}
