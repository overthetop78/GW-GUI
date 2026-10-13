using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateBandaiWonderSwan() => new(ControllerArtworkFileNames.BandaiWonderSwan,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 13.52, 39.12, 6.25, 8.82),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 15.14, 45.79, 6.25, 8.82),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 8.61, 50.52, 6.25, 8.82),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 6.58, 42.75, 6.25, 8.82),
        new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.Ellipse, 23.36, 6.78, 6.25, 8.82),
        new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.Ellipse, 25.49, 13.94, 6.25, 8.82),
        new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.Ellipse, 18.87, 18.10, 6.25, 8.82),
        new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.Ellipse, 17.03, 11.27, 6.25, 8.82),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.19, 69.09, 6.25, 8.82),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 69.69, 73.80, 6.25, 8.82),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 35.74, 68.60, 6.25, 4.41)
    ]);

    private static ProfileDefinition CreateBandaiWonderSwanColor() => new(ControllerArtworkFileNames.BandaiWonderSwanColor,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 11.68, 38.77, 6.25, 8.91),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 13.26, 45.38, 6.25, 8.91),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 6.91, 49.53, 6.25, 8.91),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 5.23, 43.20, 6.25, 8.91),
        new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.Ellipse, 21.46, 7.27, 6.25, 8.91),
        new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.Ellipse, 23.85, 14.96, 6.25, 8.91),
        new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.Ellipse, 16.95, 18.27, 6.25, 8.91),
        new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.Ellipse, 15.23, 11.92, 6.25, 8.91),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.19, 69.58, 6.25, 8.91),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 69.65, 74.23, 6.25, 8.91),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 30.27, 67.41, 6.25, 4.46)
    ]);

    private static ProfileDefinition CreateBandaiSwanCrystal() => new(ControllerArtworkFileNames.BandaiSwanCrystal,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 11.58, 40.11, 6.25, 9.09),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 13.18, 46.16, 6.25, 9.09),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 7.21, 50.94, 6.25, 9.09),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 5.02, 44.29, 6.25, 9.09),
        new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.Ellipse, 21.02, 8.41, 6.25, 9.09),
        new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.Ellipse, 22.29, 13.72, 6.25, 9.09),
        new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.Ellipse, 16.45, 18.38, 6.25, 9.09),
        new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.Ellipse, 14.45, 11.85, 6.25, 9.09),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 77.23, 69.01, 6.25, 9.09),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 70.90, 73.95, 6.25, 9.09),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 30.47, 68.75, 6.25, 4.55)
    ]);

    // The HLE core exposes eight virtual commands, not the six-button physical protocol.
    // Keep its artwork without inventing a correspondence with the photographed buttons.
    private static ProfileDefinition CreateBandaiPlaydia() => new(ControllerArtworkFileNames.BandaiPlaydia, []);

    private static ProfileDefinition CreateBandaiTamagotchi() => new(ControllerArtworkFileNames.BandaiTamagotchiP1,
    [
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 6.84, 55.67, 8.59, 8.91),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 15.43, 70.24, 8.59, 8.91),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 30.27, 79.55, 8.59, 8.91)
    ]);
}
