using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPlayStationMouse() => new(ControllerArtworkFileNames.SonyPlayStationMouse,
    [
        new(EmulationControllerVisualControl.MouseLeftButton, ControllerVisualZoneShape.RoundedRectangle, 9.0d, 4.0d, 40.0d, 38.0d),
        new(EmulationControllerVisualControl.MouseRightButton, ControllerVisualZoneShape.RoundedRectangle, 51.0d, 4.0d, 40.0d, 38.0d)
    ]);
}
