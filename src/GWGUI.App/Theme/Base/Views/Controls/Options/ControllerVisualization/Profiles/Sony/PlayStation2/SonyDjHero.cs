using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDjHero() => new(ControllerArtworkFileNames.SonyDjHero,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 10.547, 19.939, 1.758, 2.761),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 10.547, 24.233, 1.758, 2.761),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 9.180, 22.086, 1.758, 2.761),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 11.914, 22.086, 1.758, 2.761),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 27.930, 15.951, 2.734, 4.294),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 30.859, 20.552, 2.734, 4.294),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 27.930, 25.153, 2.734, 4.294),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 24.805, 20.552, 2.734, 4.294),
        new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 18.555, 20.552, 3.125, 4.908),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 16.797, 26.687, 1.367, 2.147),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 21.484, 26.687, 1.367, 2.147),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 8.984, 43.865, 4.883, 7.669),
        new(EmulationControllerVisualControl.FretGreen, ControllerVisualZoneShape.Ellipse, 45.312, 48.466, 4.297, 11.656),
        new(EmulationControllerVisualControl.FretRed, ControllerVisualZoneShape.Ellipse, 51.367, 48.160, 4.297, 11.963),
        new(EmulationControllerVisualControl.FretBlue, ControllerVisualZoneShape.Ellipse, 57.031, 48.466, 4.297, 11.656),
        new(EmulationControllerVisualControl.DialLeft, ControllerVisualZoneShape.RoundedRectangle, 26.172, 43.252, 1.953, 7.669),
        new(EmulationControllerVisualControl.DialRight, ControllerVisualZoneShape.RoundedRectangle, 28.125, 43.252, 1.953, 7.669),
        new(EmulationControllerVisualControl.CrossfaderLeft, ControllerVisualZoneShape.RoundedRectangle, 10.352, 65.031, 8.203, 5.215),
        new(EmulationControllerVisualControl.CrossfaderRight, ControllerVisualZoneShape.RoundedRectangle, 18.750, 65.031, 8.203, 5.215),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.RoundedRectangle, 54.297, 76.687, 6.836, 7.055),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.RoundedRectangle, 78.125, 76.687, 6.836, 7.055)
    ]);
}
