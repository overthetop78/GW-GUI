using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyVolumeController() => new(ControllerArtworkFileNames.SonyVolumeController,
    [
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 17, 24.6, 22.5, 12),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 61, 24.6, 22.5, 12),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.RoundedRectangle, 30, 58.5, 20, 21),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 50, 58.5, 20, 21)
    ]);
}
