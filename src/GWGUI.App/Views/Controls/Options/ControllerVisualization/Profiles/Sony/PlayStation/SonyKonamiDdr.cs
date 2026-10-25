using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyKonamiDdr() => new(ControllerArtworkFileNames.SonyKonamiDdr,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 38.9, 17.4, 21.2, 17.1),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 38.9, 78.4, 21.2, 17.1),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 5.3, 45.5, 20.2, 19),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 74.7, 45.5, 20.2, 19),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 7, 16.4, 23, 21),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 69.9, 16.4, 23, 21),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 11.4, 6.1, 10.7, 3.3),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 77.5, 6.1, 10.7, 3.3)
    ]);
}
