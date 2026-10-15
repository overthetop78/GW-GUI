using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAppleStandardJ665() => new(ControllerArtworkFileNames.AppleStandardJ665,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 64d, 1d, 29d, 61d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 64d, 1d, 29d, 61d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 64d, 1d, 29d, 61d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 64d, 1d, 29d, 61d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 13d, 5d, 16d, 23d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 9d, 37d, 19d, 29d)
        ]);
}
