using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateMattelIntellivisionController() => new(ControllerArtworkFileNames.MattelIntellivisionController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 46.78d, 56.90d, 6.21d, 3.34d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 46.78d, 73.29d, 6.21d, 3.34d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 34.61d, 64.80d, 5.25d, 3.95d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 59.90d, 64.80d, 5.25d, 3.95d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 21.72d, 26.40d, 1.91d, 5.31d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.89d, 26.40d, 1.91d, 5.31d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 21.72d, 36.12d, 1.91d, 5.31d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 75.89d, 36.12d, 1.91d, 5.31d),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 34.37d, 22.76d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 45.82d, 22.76d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 56.80d, 22.76d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 34.37d, 29.89d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 45.82d, 29.89d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 56.80d, 29.89d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 34.37d, 36.87d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 45.82d, 36.87d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 56.80d, 36.87d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.Ellipse, 34.37d, 44.16d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.Ellipse, 45.82d, 44.16d, 8.11d, 4.86d),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.Ellipse, 56.80d, 44.16d, 8.11d, 4.86d)
    ]);
}