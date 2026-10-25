using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecPcEngineLtControls() => new(ControllerArtworkFileNames.NecPcEngineLtControls,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 20.1d, 64.3d, 15.6d, 12.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 65.3d, 69.1d, 6.8d, 6.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 75.4d, 66.2d, 6.8d, 6.4d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.8d, 77.2d, 6.5d, 3.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.3d, 77.2d, 6.5d, 3.0d)
            ]);
}
