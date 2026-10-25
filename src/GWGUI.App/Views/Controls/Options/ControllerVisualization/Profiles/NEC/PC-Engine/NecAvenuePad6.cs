using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecAvenuePad6() => new(ControllerArtworkFileNames.NecAvenuePad6,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.1d, 52.8d, 18.2d, 22.6d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 65.5d, 51.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 74.3d, 51.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 83.1d, 51.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 65.5d, 67.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.3d, 67.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 83.1d, 67.0d, 7.8d, 10.6d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 37.5d, 65.6d, 8.6d, 5.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 50.7d, 65.6d, 8.0d, 5.7d)
            ]);
}
