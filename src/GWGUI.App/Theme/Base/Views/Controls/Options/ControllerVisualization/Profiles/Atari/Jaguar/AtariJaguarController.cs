using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAtariJaguarController() => new(ControllerArtworkFileNames.AtariJaguarController,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 17.5d, 16.9d, 19.3d, 23.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 71.8d, 14.6d, 10.7d, 10.9d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 65.6d, 23.5d, 10.5d, 10.6d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 59.2d, 32.7d, 11.1d, 10.4d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.RoundedRectangle, 42.4d, 32.6d, 5.1d, 7.4d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 49.0d, 32.6d, 4.8d, 7.4d),
                new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 55.1d, 7.8d, 4.0d),
                new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 55.1d, 7.6d, 4.0d),
                new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 55.1d, 7.7d, 4.0d),
                new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 63.6d, 7.8d, 4.0d),
                new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 63.6d, 7.6d, 4.0d),
                new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 63.6d, 7.7d, 4.0d),
                new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 72.3d, 7.8d, 4.0d),
                new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 72.3d, 7.6d, 4.0d),
                new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 72.3d, 7.7d, 4.0d),
                new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 35.3d, 80.7d, 7.8d, 4.1d),
                new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 46.2d, 80.7d, 7.6d, 4.1d),
                new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 56.8d, 80.7d, 7.7d, 4.1d)
            ]);
}
