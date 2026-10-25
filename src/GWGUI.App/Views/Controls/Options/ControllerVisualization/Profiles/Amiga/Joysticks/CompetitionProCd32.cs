using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateCompetitionProCd32() => new(ControllerArtworkFileNames.CompetitionProCd32,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.3d, 25.8d, 25.1d, 52.4d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.2d, 63.4d, 7.7d, 15.0d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 84.5d, 53.7d, 7.7d, 14.7d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 69.5d, 43.0d, 7.4d, 15.2d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 80.0d, 33.6d, 7.7d, 14.5d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 6.5d, 4.5d, 19.0d, 22.5d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 74.5d, 4.5d, 19.0d, 22.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 39.2d, 63.1d, 6.1d, 8.1d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 48.5d, 63.1d, 6.1d, 8.1d),
                new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.RoundedRectangle, 55.2d, 23.0d, 6.6d, 5.3d)
            ]);
}
