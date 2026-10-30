using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDualShock4() => new(ControllerArtworkFileNames.Playstation4,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 10.5d, 12.0d, 17.5d, 23.5d),
                new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 24.0d, 36.0d, 17.5d, 25.0d),
                new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 58.5d, 36.0d, 17.5d, 25.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 33.8d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.0d, 22.6d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 70.0d, 22.6d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 76.0d, 11.5d, 9.0d, 13.0d),
                new(EmulationControllerVisualControl.TouchPad, ControllerVisualZoneShape.RoundedRectangle, 34.0d, 8.0d, 32.0d, 23.0d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 28.3d, 14.5d, 3.0d, 9.0d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 68.7d, 14.5d, 3.0d, 9.0d),
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 47.0d, 43.0d, 6.0d, 9.5d)
            ]);
}
