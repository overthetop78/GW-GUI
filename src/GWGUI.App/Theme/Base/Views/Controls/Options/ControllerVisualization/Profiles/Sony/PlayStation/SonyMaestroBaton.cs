using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyMaestroBaton() => new(ControllerArtworkFileNames.SonyMaestroBaton,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.023, 30.952, 2.344, 33.333),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 78.320, 33.333, 2.539, 33.333)
    ]);
}
