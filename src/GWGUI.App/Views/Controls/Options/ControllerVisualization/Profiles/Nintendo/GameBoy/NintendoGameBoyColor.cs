using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNintendoGameBoyColor() => new(ControllerArtworkFileNames.NintendoGameBoyColor,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 17.0d, 57.0d, 20.0d, 15.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 17.0d, 57.0d, 20.0d, 15.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 17.0d, 57.0d, 20.0d, 15.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 17.0d, 57.0d, 20.0d, 15.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 56.0d, 62.3d, 11.0d, 8.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 70.0d, 59.4d, 11.0d, 8.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.0d, 78.3d, 9.0d, 4.2d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 50.0d, 78.3d, 9.0d, 4.2d)
            ]);
}
