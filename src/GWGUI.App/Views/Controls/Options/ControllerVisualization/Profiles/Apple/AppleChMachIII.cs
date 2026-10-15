using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAppleChMachIII() => new(ControllerArtworkFileNames.AppleChMachIII,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 44d, 0d, 21d, 53d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 44d, 0d, 21d, 53d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 44d, 0d, 21d, 53d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 44d, 0d, 21d, 53d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 9d, 40d, 17d, 13d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 47d, 0d, 13d, 7d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 17d, 31d, 16d, 11d)
        ]);
}
