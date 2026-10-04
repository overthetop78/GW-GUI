using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecPcEngineMouse() => new(ControllerArtworkFileNames.NecPcEngineMouse,
            [
                new(EmulationControllerVisualControl.MouseLeftButton, ControllerVisualZoneShape.RoundedRectangle, 23.3d, 18.4d, 25.1d, 27.1d),
                new(EmulationControllerVisualControl.MouseRightButton, ControllerVisualZoneShape.RoundedRectangle, 51.9d, 18.4d, 25.1d, 27.1d)
            ]);
}
