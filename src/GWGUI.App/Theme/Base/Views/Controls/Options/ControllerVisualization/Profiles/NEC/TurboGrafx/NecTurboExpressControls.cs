using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecTurboExpressControls() => new(ControllerArtworkFileNames.NecTurboexpressControls,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 19.5d, 58.1d, 18.5d, 10.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 60.3d, 62.3d, 9.0d, 6.3d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72.1d, 57.6d, 9.0d, 6.3d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.0d, 84.3d, 8.3d, 3.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.2d, 84.3d, 8.3d, 3.3d)
            ]);
}
