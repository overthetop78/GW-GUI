using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateThomsonJoystick() => new(ControllerArtworkFileNames.ThomsonMj90102, [new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 45d, 0d, 16d, 26d), new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 45d, 0d, 16d, 26d), new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 45d, 0d, 16d, 26d), new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 45d, 0d, 16d, 26d), new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 18d, 12d, 21d, 14d)]);
}
