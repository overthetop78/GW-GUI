using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyGunCon2() => new(ControllerArtworkFileNames.SonyGunCon2,
    [
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 55.8d, 30.3d, 5.0d, 9.2d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 46.8d, 19.8d, 9.0d, 5.5d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 37.4d, 24.8d, 3.2d, 3.5d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 32.1d, 25.2d, 3.2d, 3.5d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 74.0d, 64.5d, 10.5d, 3.0d)
    ]);
}
