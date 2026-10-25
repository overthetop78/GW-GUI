using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecPcEngineTurboPadIi() => new(ControllerArtworkFileNames.NecPcEngineTurbopadIi,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 9.5d, 50.3d, 18.0d, 25.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.5d, 62.1d, 8.6d, 14.1d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.0d, 62.1d, 8.6d, 14.1d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.0d, 66.3d, 8.3d, 6.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 66.3d, 8.3d, 6.3d)
            ]);
}
