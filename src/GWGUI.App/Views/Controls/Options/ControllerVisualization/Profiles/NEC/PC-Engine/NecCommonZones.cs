using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static readonly IReadOnlyList<ControllerVisualZone> NecCoreGrafxTurboPadZones =
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.7d, 52.4d, 16.5d, 23.1d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 69.7d, 62.8d, 9.1d, 11.8d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80.8d, 62.8d, 9.1d, 11.8d),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 40.4d, 66.1d, 8.1d, 5.7d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.2d, 66.1d, 8.1d, 5.7d)
    ];
}
