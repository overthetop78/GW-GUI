using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyGunCon() => new(ControllerArtworkFileNames.SonyGunCon,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 47.0d, 39.0d, 14.0d, 14.0d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 11.0d, 25.0d, 10.0d, 12.0d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 60.0d, 29.0d, 8.0d, 5.0d)
    ]);
}
