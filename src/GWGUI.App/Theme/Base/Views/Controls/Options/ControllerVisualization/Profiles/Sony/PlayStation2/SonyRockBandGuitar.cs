using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyRockBandGuitar() => new(ControllerArtworkFileNames.SonyRockBandGuitar,
    [
        new(EmulationControllerVisualControl.FretGreen, ControllerVisualZoneShape.RoundedRectangle, 43.878, 16.797, 11.735, 3.516),
        new(EmulationControllerVisualControl.SoloFretGreen, ControllerVisualZoneShape.RoundedRectangle, 43.367, 50.781, 12.755, 1.758),
        new(EmulationControllerVisualControl.FretRed, ControllerVisualZoneShape.RoundedRectangle, 43.878, 20.508, 11.735, 3.516),
        new(EmulationControllerVisualControl.SoloFretRed, ControllerVisualZoneShape.RoundedRectangle, 43.367, 52.734, 12.755, 1.758),
        new(EmulationControllerVisualControl.FretYellow, ControllerVisualZoneShape.RoundedRectangle, 43.878, 24.219, 11.735, 3.516),
        new(EmulationControllerVisualControl.SoloFretYellow, ControllerVisualZoneShape.RoundedRectangle, 43.367, 54.688, 12.755, 1.758),
        new(EmulationControllerVisualControl.FretBlue, ControllerVisualZoneShape.RoundedRectangle, 43.878, 27.930, 11.735, 3.516),
        new(EmulationControllerVisualControl.SoloFretBlue, ControllerVisualZoneShape.RoundedRectangle, 43.367, 56.641, 12.755, 1.758),
        new(EmulationControllerVisualControl.FretOrange, ControllerVisualZoneShape.RoundedRectangle, 43.878, 31.641, 11.735, 3.516),
        new(EmulationControllerVisualControl.SoloFretOrange, ControllerVisualZoneShape.RoundedRectangle, 43.367, 58.594, 12.755, 1.758),
        new(EmulationControllerVisualControl.StrumUp, ControllerVisualZoneShape.RoundedRectangle, 46.429, 65.039, 3.061, 7.812),
        new(EmulationControllerVisualControl.StrumDown, ControllerVisualZoneShape.RoundedRectangle, 50.000, 65.039, 3.061, 7.812),
        new(EmulationControllerVisualControl.Whammy, ControllerVisualZoneShape.RoundedRectangle, 77.551, 70.508, 7.653, 5.859),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 64.286, 74.805, 6.122, 2.734),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 73.469, 77.344, 6.633, 2.734),
        new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 65.306, 84.766, 6.122, 2.539),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 83.673, 80.078, 4.082, 1.562),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 83.673, 83.008, 4.082, 1.562),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 80.102, 81.445, 4.082, 1.562),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 87.755, 81.445, 4.082, 1.562),
        new(EmulationControllerVisualControl.EffectPosition1, ControllerVisualZoneShape.Ellipse, 73.469, 87.891, 9.184, 4.688),
        new(EmulationControllerVisualControl.EffectPosition2, ControllerVisualZoneShape.Ellipse, 73.469, 87.891, 9.184, 4.688),
        new(EmulationControllerVisualControl.EffectPosition3, ControllerVisualZoneShape.Ellipse, 73.469, 87.891, 9.184, 4.688),
        new(EmulationControllerVisualControl.EffectPosition4, ControllerVisualZoneShape.Ellipse, 73.469, 87.891, 9.184, 4.688),
        new(EmulationControllerVisualControl.EffectPosition5, ControllerVisualZoneShape.Ellipse, 73.469, 87.891, 9.184, 4.688)
    ]);
}
