using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNintendoNesAdvantage() => new(ControllerArtworkFileNames.NintendoNesAdvantage,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 20.0d, 22.0d, 16.5d, 22.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 20.0d, 22.0d, 16.5d, 22.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 20.0d, 22.0d, 16.5d, 22.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 20.0d, 22.0d, 16.5d, 22.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 49.0d, 47.9d, 9.2d, 13.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 61.3d, 43.2d, 9.3d, 13.3d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 47.5d, 37.2d, 6.5d, 6.8d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.RoundedRectangle, 60.0d, 31.2d, 6.5d, 6.8d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 73.5d, 33.7d, 6.8d, 6.1d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 74.3d, 42.5d, 6.8d, 6.1d)
            ]);
}
