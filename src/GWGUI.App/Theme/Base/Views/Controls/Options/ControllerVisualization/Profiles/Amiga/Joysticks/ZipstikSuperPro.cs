using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateZipstikSuperPro() => new(ControllerArtworkFileNames.ZipstikSuperPro,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 22.1d, 39.7d, 56.1d, 41.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 7.4d, 5.1d, 21.9d, 16.8d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 71.0d, 5.1d, 21.7d, 16.8d)
            ]);
}
