using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAppleChMachII() => new(ControllerArtworkFileNames.AppleChMachII,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 57d, 0d, 12d, 48d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 57d, 0d, 12d, 48d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 57d, 0d, 12d, 48d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 57d, 0d, 12d, 48d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 10d, 31d, 17d, 12d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 17d, 22d, 16d, 10d)
        ]);
}
