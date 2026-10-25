using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyGuitarFreaks() => new(ControllerArtworkFileNames.SonyGuitarFreaks,
    [
        new(EmulationControllerVisualControl.FretRed, ControllerVisualZoneShape.RoundedRectangle, 47.2, 17.8, 12.4, 2.7),
        new(EmulationControllerVisualControl.FretGreen, ControllerVisualZoneShape.RoundedRectangle, 47.2, 21.7, 12.4, 2.7),
        new(EmulationControllerVisualControl.FretBlue, ControllerVisualZoneShape.RoundedRectangle, 47.2, 25.4, 12.4, 2.7),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 49.2, 68.5, 8.3, 7.8),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 72.5, 84.1, 7.8, 2.9),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 77.7, 88.3, 7.8, 2.9)
    ]);
}
