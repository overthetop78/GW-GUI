using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateMattelIntellivisionController() => new(ControllerArtworkFileNames.MattelIntellivisionController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 70.51d, 63.28d, 5.47d, 4.49d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 70.51d, 78.71d, 5.47d, 4.49d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 62.5d, 70.31d, 4.49d, 5.47d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 79.69d, 70.31d, 4.49d, 5.47d),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 24.41d, 59.38d, 5.08d, 3.91d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 24.41d, 72.07d, 5.08d, 3.91d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 15.43d, 65.43d, 3.91d, 4.69d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 34.38d, 65.43d, 3.91d, 4.69d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 1.17d, 31.05d, 2.54d, 7.62d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 95.9d, 26.76d, 2.93d, 8.4d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 0.98d, 42.58d, 2.54d, 6.25d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 95.31d, 38.09d, 2.93d, 6.84d),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 64.26d, 23.24d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 13.28d, 21.09d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 72.85d, 21.09d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 22.66d, 24.02d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 81.64d, 19.14d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 32.03d, 26.37d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 63.87d, 31.84d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 13.28d, 28.13d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 72.46d, 29.3d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 22.66d, 30.66d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 81.64d, 27.54d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 32.03d, 33.2d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 63.28d, 40.23d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 13.09d, 35.16d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 72.07d, 38.28d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 22.66d, 37.5d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 81.45d, 36.13d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 32.03d, 40.04d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.Ellipse, 62.89d, 48.83d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.Ellipse, 13.09d, 42.38d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.Ellipse, 71.68d, 47.07d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.Ellipse, 22.66d, 44.73d, 5.47d, 4.69d),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.Ellipse, 81.25d, 44.53d, 5.47d, 5.08d),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.Ellipse, 32.03d, 47.46d, 5.47d, 4.69d)
    ]);
}