using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateQuickShot() => new(ControllerArtworkFileNames.Quickshot,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 22.6d, 0.0d, 54.0d, 52.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 43.8d, 4.3d, 13.4d, 28.0d)
            ]);
}
