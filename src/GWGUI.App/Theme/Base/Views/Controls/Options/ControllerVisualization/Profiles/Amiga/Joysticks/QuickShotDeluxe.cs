using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateQuickShotDeluxe() => new(ControllerArtworkFileNames.QuickshotDeluxe,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 6.0d, 0.0d, 88.4d, 49.2d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 37.4d, 6.7d, 24.4d, 14.5d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 15.6d, 7.5d, 14.4d, 11.7d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 69.3d, 7.6d, 14.6d, 11.5d)
            ]);
}
