using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyGuitarHeroSg() => new(ControllerArtworkFileNames.SonyGuitarHeroSg,
    [
        new(EmulationControllerVisualControl.FretGreen, ControllerVisualZoneShape.RoundedRectangle, 40.3, 14.4, 13.8, 2.8),
        new(EmulationControllerVisualControl.FretRed, ControllerVisualZoneShape.RoundedRectangle, 40.3, 17.9, 13.8, 2.8),
        new(EmulationControllerVisualControl.FretYellow, ControllerVisualZoneShape.RoundedRectangle, 40.3, 21.4, 13.8, 2.8),
        new(EmulationControllerVisualControl.FretBlue, ControllerVisualZoneShape.RoundedRectangle, 40.3, 25, 13.8, 2.8),
        new(EmulationControllerVisualControl.FretOrange, ControllerVisualZoneShape.RoundedRectangle, 40.3, 28.5, 13.8, 2.8),
        new(EmulationControllerVisualControl.StrumUp, ControllerVisualZoneShape.RoundedRectangle, 46.4, 71.5, 7.2, 7),
        new(EmulationControllerVisualControl.StrumDown, ControllerVisualZoneShape.RoundedRectangle, 46.4, 78.5, 7.2, 7),
        new(EmulationControllerVisualControl.Whammy, ControllerVisualZoneShape.RoundedRectangle, 74.5, 75.2, 17.7, 16),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 57.8, 93.2, 9.4, 3.5),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 51.4, 89.9, 9.4, 3.5)
    ]);
}
