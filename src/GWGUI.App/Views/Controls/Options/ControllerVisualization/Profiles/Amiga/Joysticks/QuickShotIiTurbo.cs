using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateQuickShotIiTurbo() => new(ControllerArtworkFileNames.QuickshotIiTurbo,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 33.3d, 8.9d, 33.4d, 61.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 39.9d, 15.6d, 20.0d, 41.4d)
            ]);
}
