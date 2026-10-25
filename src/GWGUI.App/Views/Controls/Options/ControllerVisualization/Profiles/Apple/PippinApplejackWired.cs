using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    // Physical zones only: no catalogued core currently supports the Pippin.
    private static ProfileDefinition CreatePippinApplejackWired() => new(ControllerArtworkFileNames.PippinApplejackWired,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 31d, 14d, 20d, 17d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 31d, 14d, 20d, 17d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 31d, 14d, 20d, 17d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 31d, 14d, 20d, 17d),
            new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 60d, 20d, 5d, 5d),
            new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 60d, 29d, 5d, 5d),
            new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 56d, 24d, 5d, 5d),
            new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 65d, 24d, 5d, 5d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 82d, 37d, 6d, 6d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 72d, 39d, 6d, 6d),
            new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 85d, 30d, 6d, 6d),
            new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75d, 30d, 6d, 6d),
            new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 42d, 42d, 5d, 5d),
            new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 50d, 45d, 5d, 5d),
            new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 60d, 50d, 5d, 5d)
        ]);
}
