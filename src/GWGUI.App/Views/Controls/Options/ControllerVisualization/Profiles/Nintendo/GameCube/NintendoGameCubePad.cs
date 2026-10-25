using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNintendoGameCubePad() => new(ControllerArtworkFileNames.NintendoGamecube,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 24.0d, 63.0d, 13.0d, 17.0d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 24.0d, 63.0d, 13.0d, 17.0d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 24.0d, 63.0d, 13.0d, 17.0d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 24.0d, 63.0d, 13.0d, 17.0d),
                new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 15.0d, 42.0d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 15.0d, 53.0d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 11.0d, 47.5d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 19.0d, 47.5d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.CUp, ControllerVisualZoneShape.Ellipse, 57.0d, 65.5d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.CDown, ControllerVisualZoneShape.Ellipse, 57.0d, 77.0d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.CLeft, ControllerVisualZoneShape.Ellipse, 53.0d, 71.5d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.CRight, ControllerVisualZoneShape.Ellipse, 61.0d, 71.5d, 3.0d, 4.0d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 56.6d, 50.8d, 7.0d, 8.8d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 64.1d, 43.3d, 10.3d, 12.4d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 61.7d, 34.2d, 10.3d, 8.2d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75.6d, 40.6d, 6.8d, 10.7d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 41.7d, 47.5d, 4.6d, 5.8d)
            ]);
}
