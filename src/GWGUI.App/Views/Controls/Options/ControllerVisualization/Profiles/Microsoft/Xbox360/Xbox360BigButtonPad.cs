using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateXbox360BigButtonPad() => new(ControllerArtworkFileNames.Xbox360BigButtonPad,
            [
                new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 44.3d, 81.8d, 11.4d, 8.6d),
                new(EmulationControllerVisualControl.Buzzer, ControllerVisualZoneShape.Ellipse, 27.3d, 2.7d, 45.5d, 26.8d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 32.0d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 42.0d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 52.0d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 44.2d, 62.1d, 11.2d, 8.6d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 39.3d, 72.7d, 6.1d, 5.3d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 53.5d, 72.7d, 6.1d, 5.3d)
            ]);
}
