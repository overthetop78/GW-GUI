using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSegaArcadePowerStick3() => new(ControllerArtworkFileNames.SegaArcadePowerStick3,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 27d, 31d, 26d, 41d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 27d, 31d, 26d, 41d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 27d, 31d, 26d, 41d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 27d, 31d, 26d, 41d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 63d, 53d, 12d, 15d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 73d, 46d, 12d, 15d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 84d, 42d, 12d, 15d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 47d, 16d, 10d, 7d)
            ]);
}
