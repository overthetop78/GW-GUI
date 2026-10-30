using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNintendo64Pad() => new(ControllerArtworkFileNames.Nintendo64,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 22.7d, 27.8d, 12.8d, 17.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 22.7d, 27.8d, 12.8d, 17.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 22.7d, 27.8d, 12.8d, 17.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 22.7d, 27.8d, 12.8d, 17.4d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 48.8d, 47.7d, 5.0d, 6.7d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 48.8d, 56.0d, 5.0d, 6.7d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 45.9d, 51.9d, 5.0d, 6.7d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 51.8d, 51.9d, 5.0d, 6.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 66.5d, 40.5d, 5.2d, 7.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 61.7d, 33.4d, 5.2d, 7.4d),
                new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.Ellipse, 72.7d, 22.1d, 5.1d, 6.9d),
                new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.Ellipse, 72.7d, 35.0d, 5.1d, 6.9d),
                new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.Ellipse, 68.5d, 28.5d, 5.1d, 6.9d),
                new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.Ellipse, 77.0d, 28.5d, 5.1d, 6.9d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 21.3d, 15.3d, 5.8d, 4.2d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 73.2d, 15.3d, 7.0d, 4.2d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 48.7d, 32.3d, 5.2d, 7.4d)
            ]);
}
