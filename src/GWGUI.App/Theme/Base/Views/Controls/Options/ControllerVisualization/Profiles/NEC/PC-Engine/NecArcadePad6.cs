using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecArcadePad6() => new(ControllerArtworkFileNames.NecArcadePad6,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 5.7d, 54.7d, 15.8d, 29.4d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 65.9d, 52.6d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 75.5d, 52.6d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 84.7d, 52.6d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 65.9d, 68.5d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 75.5d, 68.5d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 84.7d, 68.5d, 8.1d, 13.7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.1d, 73.5d, 7.2d, 6.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 51.5d, 73.5d, 7.2d, 6.7d)
            ]);
}
