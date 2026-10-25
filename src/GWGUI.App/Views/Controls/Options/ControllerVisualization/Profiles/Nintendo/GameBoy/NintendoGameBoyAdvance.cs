using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNintendoGameBoyAdvance() => new(ControllerArtworkFileNames.NintendoGameBoyAdvance,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.2d, 30.0d, 14.0d, 23.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.2d, 30.0d, 14.0d, 23.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.2d, 30.0d, 14.0d, 23.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.2d, 30.0d, 14.0d, 23.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.8d, 37.2d, 7.0d, 12.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 85.0d, 32.5d, 7.0d, 12.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 18.0d, 68.0d, 4.5d, 8.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 18.0d, 60.0d, 4.5d, 8.0d)
            ]);
}
