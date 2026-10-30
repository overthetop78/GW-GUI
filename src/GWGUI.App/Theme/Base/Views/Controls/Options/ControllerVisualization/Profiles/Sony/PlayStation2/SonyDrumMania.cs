using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDrumMania() => new(ControllerArtworkFileNames.SonyDrumMania,
    [
        new(EmulationControllerVisualControl.DrumHiHat, ControllerVisualZoneShape.Ellipse, 3.125, 28.986, 13.086, 19.130),
        new(EmulationControllerVisualControl.DrumSnare, ControllerVisualZoneShape.Ellipse, 21.094, 37.391, 21.875, 32.174),
        new(EmulationControllerVisualControl.DrumHighTom, ControllerVisualZoneShape.Ellipse, 26.172, 2.899, 21.680, 31.304),
        new(EmulationControllerVisualControl.DrumLowTom, ControllerVisualZoneShape.Ellipse, 50.195, 2.899, 21.484, 31.304),
        new(EmulationControllerVisualControl.DrumCymbal, ControllerVisualZoneShape.Ellipse, 80.273, 4.928, 14.062, 18.841),
        new(EmulationControllerVisualControl.DrumKick, ControllerVisualZoneShape.Ellipse, 48.242, 70.725, 27.344, 22.319),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 47.266, 38.261, 4.688, 7.246),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 54.297, 38.261, 4.688, 7.246)
    ]);
}
