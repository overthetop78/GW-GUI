using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecAvenuePad3() => new(ControllerArtworkFileNames.NecAvenuePad3,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 65.3d, 62.7d, 8.7d, 13.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.5d, 62.7d, 8.7d, 13.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.7d, 62.7d, 8.7d, 13.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 37.5d, 65.6d, 8.6d, 5.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 50.7d, 65.6d, 8.0d, 5.7d)
            ]);
}
