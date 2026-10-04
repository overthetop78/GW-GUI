using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateAtariCx40() => new(ControllerArtworkFileNames.AtariCx40,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 26.6d, 28.1d, 46.3d, 44.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 15.8d, 14.4d, 14.5d, 14.4d)
            ]);
}
