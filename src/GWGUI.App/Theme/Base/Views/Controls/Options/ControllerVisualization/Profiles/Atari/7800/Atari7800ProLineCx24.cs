using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAtari7800ProLineCx24() => new(ControllerArtworkFileNames.Atari7800ProLineCx24,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 36.8d, 27.4d, 25.7d, 18.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 26.2d, 11.9d, 6.8d, 16.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 66.2d, 11.7d, 6.5d, 17.0d)
            ]);
}
