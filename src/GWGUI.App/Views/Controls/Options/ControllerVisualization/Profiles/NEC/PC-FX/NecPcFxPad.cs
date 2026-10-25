using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecPcFxPad() => new(ControllerArtworkFileNames.NecPcFxPad,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.8d, 39.8d, 16.3d, 34.2d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 68.9d, 43.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 77.9d, 43.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 86.8d, 43.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 69.0d, 65.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 77.9d, 65.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 86.8d, 65.2d, 6.4d, 13.5d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.3d, 69.6d, 6.6d, 5.8d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 54.1d, 69.6d, 6.6d, 5.8d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 40.2d, 52.5d, 5.0d, 5.6d)
            ]);
}
