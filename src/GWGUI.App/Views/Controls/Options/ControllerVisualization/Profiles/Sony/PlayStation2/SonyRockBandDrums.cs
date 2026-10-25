using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyRockBandDrums() => new(ControllerArtworkFileNames.SonyRockBandDrums,
    [
        new(EmulationControllerVisualControl.DrumRed, ControllerVisualZoneShape.Ellipse, 3.906, 17.206, 20.898, 20.243),
        new(EmulationControllerVisualControl.DrumYellow, ControllerVisualZoneShape.Ellipse, 25.391, 2.429, 20.312, 19.028),
        new(EmulationControllerVisualControl.DrumBlue, ControllerVisualZoneShape.Ellipse, 55.273, 2.429, 20.312, 19.028),
        new(EmulationControllerVisualControl.DrumGreen, ControllerVisualZoneShape.Ellipse, 75.000, 17.206, 20.898, 20.243),
        new(EmulationControllerVisualControl.DrumKick, ControllerVisualZoneShape.Ellipse, 46.289, 70.445, 8.203, 21.255),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 46.094, 22.874, 0.977, 1.012),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 46.094, 24.494, 0.977, 1.012),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 45.312, 23.684, 0.977, 1.012),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 46.875, 23.684, 0.977, 1.012),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 47.070, 20.243, 1.172, 1.215),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 51.172, 20.243, 1.172, 1.215),
        new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 48.828, 20.040, 1.953, 1.619),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 51.953, 21.862, 1.367, 1.215),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 53.711, 23.077, 1.367, 1.215),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 51.953, 24.494, 1.367, 1.215),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 50.000, 23.077, 1.367, 1.215)
    ]);
}
