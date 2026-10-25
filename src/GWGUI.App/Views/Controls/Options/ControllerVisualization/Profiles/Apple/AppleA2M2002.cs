using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAppleA2M2002() => new(ControllerArtworkFileNames.AppleA2M2002,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 30d, 16d, 63d, 62d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 30d, 16d, 63d, 62d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 30d, 16d, 63d, 62d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 30d, 16d, 63d, 62d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 5d, 22d, 18d, 17d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 25d, 5d, 18d, 16d)
        ]);
}
