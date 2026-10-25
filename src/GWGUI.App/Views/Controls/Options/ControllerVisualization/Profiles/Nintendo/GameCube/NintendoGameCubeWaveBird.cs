using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNintendoGameCubeWaveBird() => new(ControllerArtworkFileNames.NintendoGamecubeWavebird,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 28.5d, 56.3d, 12.4d, 15.9d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 28.5d, 56.3d, 12.4d, 15.9d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 28.5d, 56.3d, 12.4d, 15.9d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 28.5d, 56.3d, 12.4d, 15.9d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 19.6d, 32.9d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 19.6d, 44.6d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 15.2d, 39.0d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 24.0d, 39.0d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.Ellipse, 62.6d, 56.9d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.Ellipse, 62.6d, 67.9d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.Ellipse, 58.3d, 62.5d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.Ellipse, 66.9d, 62.5d, 3.3d, 4.8d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 62.6d, 42.6d, 7.2d, 9.6d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 70.5d, 33.8d, 12.0d, 14.8d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 68.3d, 24.5d, 11.0d, 9.6d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 83.8d, 31.3d, 7.5d, 13.2d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 46.9d, 41.5d, 5.5d, 6.1d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 74.4d, 16.0d, 12.0d, 7.5d)
            ]);
}
