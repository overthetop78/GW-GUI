using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateBandaiWonderSwan(string file) => new(file,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 11, 40, 8, 9),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 14, 48, 8, 9),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 7, 51, 8, 9),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 4, 43, 8, 9),
        new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.Ellipse, 20, 7, 8, 9),
        new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.Ellipse, 23, 14, 8, 9),
        new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.Ellipse, 16, 18, 8, 9),
        new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.Ellipse, 13, 10, 8, 9),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 77, 70, 8, 10),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 69, 76, 9, 10),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, file == ControllerArtworkFileNames.BandaiWonderSwan ? 35 : 29, 67, 10, 9)
    ]);

    // The HLE core exposes eight virtual commands, not the six-button physical protocol.
    // Keep its artwork without inventing a correspondence with the photographed buttons.
    private static ProfileDefinition CreateBandaiPlaydia() => new(ControllerArtworkFileNames.BandaiPlaydia, []);

    private static ProfileDefinition CreateBandaiTamagotchi() => new(ControllerArtworkFileNames.BandaiTamagotchiP1,
    [
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 6, 55, 10, 12),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 14, 69, 11, 13),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 29, 79, 12, 13)
    ]);
}
