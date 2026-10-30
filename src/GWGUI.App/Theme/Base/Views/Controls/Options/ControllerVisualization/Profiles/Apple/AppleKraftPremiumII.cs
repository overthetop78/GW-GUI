using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAppleKraftPremiumII() => new(ControllerArtworkFileNames.AppleKraftPremiumII,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 55d, 31d, 15d, 16d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 55d, 31d, 15d, 16d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 55d, 31d, 15d, 16d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 55d, 31d, 15d, 16d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 9d, 17d, 11d, 13d)
        ]);
}
