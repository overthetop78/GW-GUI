using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAtari5200Controller() => new(ControllerArtworkFileNames.Atari5200Controller,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 36.4d, 20.0d, 26.4d, 23.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 36.4d, 20.0d, 26.4d, 23.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 36.4d, 20.0d, 26.4d, 23.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 36.4d, 20.0d, 26.4d, 23.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 21.3d, 17.4d, 3.3d, 6.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 75.5d, 17.4d, 3.3d, 6.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 16.5d, 17.4d, 3.3d, 6.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 80.2d, 17.4d, 3.3d, 6.9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 37.0d, 12.0d, 7.0d, 5.0d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.RoundedRectangle, 46.3d, 12.0d, 7.1d, 5.0d),
                new(EmulationControllerVisualControl.Reset, ControllerVisualZoneShape.RoundedRectangle, 55.6d, 12.0d, 7.1d, 5.0d),
                new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 37.9d, 55.3d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 46.3d, 55.3d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 54.6d, 55.3d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 37.9d, 62.3d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 46.3d, 62.3d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 54.6d, 62.3d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 37.9d, 69.4d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 46.3d, 69.4d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 54.6d, 69.4d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 37.9d, 76.5d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 46.3d, 76.5d, 6.0d, 4.8d),
                new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 54.6d, 76.5d, 6.0d, 4.8d)
            ]);
}
