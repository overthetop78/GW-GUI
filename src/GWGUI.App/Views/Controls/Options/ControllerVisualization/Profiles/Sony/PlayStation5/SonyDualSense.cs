using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualSense() => new(ControllerArtworkFileNames.Playstation5,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 11.3d, 17.5d, 15.5d, 26.0d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 27.0d, 43.0d, 15.0d, 22.0d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 58.0d, 43.0d, 15.0d, 22.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 36.0d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.2d, 26.0d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 68.8d, 26.0d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 16.2d, 7.8d, 11.8d),
                new(EmulationControllerVisualControl.TouchPad, ControllerVisualZoneShape.RoundedRectangle, 33.0d, 7.2d, 34.0d, 29.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 26.4d, 14.0d, 3.0d, 10.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 71.0d, 14.0d, 3.0d, 10.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 47.0d, 49.0d, 6.0d, 9.5d)
            ]);
}
