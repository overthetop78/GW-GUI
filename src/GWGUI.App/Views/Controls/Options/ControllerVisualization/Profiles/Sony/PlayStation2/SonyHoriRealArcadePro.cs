using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyHoriRealArcadePro() => new(ControllerArtworkFileNames.SonyHoriRealArcadePro,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 26.56, 24.42, 9.77, 16.5),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 26.56, 24.42, 9.77, 16.5),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 26.56, 24.42, 9.77, 16.5),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 26.56, 24.42, 9.77, 16.5),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 46.88, 31.68, 7.42, 12.54),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 56.45, 28.38, 7.42, 12.54),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 66.02, 28.38, 7.42, 12.54),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 75.78, 28.38, 7.42, 12.54),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 46.48, 45.21, 7.42, 12.54),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 56.05, 42.24, 7.42, 12.54),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 65.62, 42.9, 7.42, 12.54),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 75.59, 42.57, 7.42, 12.54),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 71.29, 9.9, 6.25, 10.56),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 80.47, 9.9, 6.25, 10.56),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 21.88, 16.17, 0.78, 2.64),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 23.83, 16.17, 0.78, 2.64),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 25.98, 16.17, 0.78, 2.64),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 27.93, 16.17, 0.78, 2.64),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 30.08, 16.17, 0.78, 2.64),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 32.23, 16.17, 0.78, 2.64),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 34.18, 16.17, 0.78, 2.64),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 36.33, 16.17, 0.78, 2.64)
    ]);
}
