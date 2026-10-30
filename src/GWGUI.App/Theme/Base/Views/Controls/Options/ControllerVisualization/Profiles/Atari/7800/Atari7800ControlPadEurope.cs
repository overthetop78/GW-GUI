using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAtari7800ControlPadEurope() => new(ControllerArtworkFileNames.Atari7800ControlPadEurope,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 14.8d, 17.0d, 19.4d, 29.9d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 46.4d, 51.4d, 10.3d, 16.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 64.8d, 51.4d, 10.3d, 16.0d)
            ]);
}
