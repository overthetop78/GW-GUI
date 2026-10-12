using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    // Jose Antonio Ortigueira Amor, original photograph cropped and background removed along the actual contour.
    // https://retroordenadoresorty.blogspot.com/2024/10/joystick-thomson-mj-90-102-to7-y-to770-1.html
    private static ProfileDefinition CreateThomsonJoystick() => new(
        ControllerArtworkFileNames.ThomsonMj90102,
        [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 44d, 1d, 17d, 27d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 44d, 1d, 17d, 27d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 44d, 1d, 17d, 27d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 44d, 1d, 17d, 27d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 18d, 13d, 22d, 13d)
    ]
    );
}
