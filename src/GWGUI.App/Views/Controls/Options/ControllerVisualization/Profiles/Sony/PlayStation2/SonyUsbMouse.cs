using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyUsbMouse() => new(ControllerArtworkFileNames.SonyUsbMouse,
    [
        new(EmulationControllerVisualControl.MouseLeftButton, ControllerVisualZoneShape.Ellipse, 13, 1, 31, 23),
        new(EmulationControllerVisualControl.MouseRightButton, ControllerVisualZoneShape.Ellipse, 56, 1, 31, 23),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 43, 9.5, 13, 11)
    ]);
}
