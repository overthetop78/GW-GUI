using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXboxDvdMoviePlaybackKit() => new(ControllerArtworkFileNames.XboxDvdMoviePlaybackKit,
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 33.0d, 4.1d, 8.1d, 10.5d),
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 34.6d, 16.5d, 6.6d, 5.4d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 34.6d, 30.0d, 6.6d, 5.4d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 29.5d, 22.0d, 5.9d, 6.7d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 40.3d, 22.0d, 5.9d, 6.7d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 34.5d, 22.1d, 6.8d, 7.5d),
                new(EmulationControllerVisualControl.Rewind, ControllerVisualZoneShape.Ellipse, 29.9d, 37.5d, 4.8d, 5.3d),
                new(EmulationControllerVisualControl.FastForward, ControllerVisualZoneShape.Ellipse, 40.9d, 37.5d, 4.8d, 5.3d),
                new(EmulationControllerVisualControl.PlayPause, ControllerVisualZoneShape.Ellipse, 34.4d, 36.5d, 6.6d, 6.5d),
                new(EmulationControllerVisualControl.Pause, ControllerVisualZoneShape.Ellipse, 34.5d, 44.0d, 6.5d, 5.5d),
                new(EmulationControllerVisualControl.Stop, ControllerVisualZoneShape.Ellipse, 34.5d, 51.2d, 6.5d, 5.5d)
            ]);
}
