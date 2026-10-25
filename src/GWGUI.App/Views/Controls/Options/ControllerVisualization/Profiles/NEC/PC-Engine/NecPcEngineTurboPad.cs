using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecPcEngineTurboPad() => new(ControllerArtworkFileNames.NecPcEngineTurbopad,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.1d, 16.5d, 24.3d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.4d, 62.3d, 8.8d, 13.2d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.9d, 62.3d, 8.8d, 13.2d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.2d, 66.0d, 8.0d, 5.1d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.9d, 66.0d, 8.0d, 5.1d)
            ]);
}
