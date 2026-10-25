using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecPcFxMouse() => new(ControllerArtworkFileNames.NecPcFxMouse,
            [
                new(EmulationControllerVisualControl.MouseLeftButton, ControllerVisualZoneShape.RoundedRectangle, 21.4d, 18.0d, 27.5d, 27.5d),
                new(EmulationControllerVisualControl.MouseRightButton, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 18.0d, 27.5d, 27.5d)
            ]);
}
