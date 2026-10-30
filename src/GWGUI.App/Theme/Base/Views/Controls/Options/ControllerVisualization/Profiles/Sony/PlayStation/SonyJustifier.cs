using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyJustifier() => new(ControllerArtworkFileNames.SonyJustifier,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 60.0d, 39.0d, 10.0d, 18.0d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 60.0d, 29.0d, 5.0d, 4.0d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 59.0d, 17.0d, 8.0d, 8.0d)
    ]);
}
