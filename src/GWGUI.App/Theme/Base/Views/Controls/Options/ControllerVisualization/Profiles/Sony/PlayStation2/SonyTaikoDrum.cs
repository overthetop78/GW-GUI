using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyTaikoDrum() => new(ControllerArtworkFileNames.SonyTaikoDrum,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 17, 14, 30, 52),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 53, 14, 30, 52),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 3, 30, 7, 22),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 90, 30, 7, 22),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 38.3, 79.2, 6.7, 7.6),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 54.5, 79.2, 6.7, 7.6)
    ]);
}
