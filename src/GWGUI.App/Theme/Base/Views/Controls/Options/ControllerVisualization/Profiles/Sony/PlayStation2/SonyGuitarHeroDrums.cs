using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyGuitarHeroDrums() => new(ControllerArtworkFileNames.SonyGuitarHeroDrums,
    [
        new(EmulationControllerVisualControl.DrumRed, ControllerVisualZoneShape.Ellipse, 2.564, 24.609, 26.496, 6.836),
        new(EmulationControllerVisualControl.DrumBlue, ControllerVisualZoneShape.Ellipse, 37.892, 22.461, 24.786, 6.836),
        new(EmulationControllerVisualControl.DrumGreen, ControllerVisualZoneShape.Ellipse, 72.365, 24.609, 25.641, 6.836),
        new(EmulationControllerVisualControl.DrumYellow, ControllerVisualZoneShape.Ellipse, 7.123, 4.297, 32.479, 6.250),
        new(EmulationControllerVisualControl.DrumOrange, ControllerVisualZoneShape.Ellipse, 60.114, 4.297, 32.764, 6.250),
        new(EmulationControllerVisualControl.DrumKick, ControllerVisualZoneShape.Ellipse, 43.875, 86.133, 12.251, 11.523),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 41.026, 17.188, 1.140, 0.781),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 41.026, 18.359, 1.140, 0.781),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 39.886, 17.773, 1.140, 0.781),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 42.165, 17.773, 1.140, 0.781),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 44.729, 17.383, 1.709, 0.977),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 51.852, 17.383, 1.709, 0.977),
        new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 47.863, 17.188, 2.849, 1.367),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 56.695, 16.406, 1.709, 0.977),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 58.405, 17.383, 1.709, 0.977),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 56.695, 18.359, 1.709, 0.977),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 54.986, 17.383, 1.709, 0.977)
    ]);
}
