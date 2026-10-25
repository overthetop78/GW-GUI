using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    // Apple PPBD-JHACEF003, figure 5: S7 green, S8 red, S9 blue, S10 yellow.
    // These zones describe the photograph, not support by an emulation core.
    private static ProfileDefinition CreatePippinApplejack() => new(ControllerArtworkFileNames.PippinApplejack,
        [
            new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 11d, 22d, 21d, 25d),
            new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 11d, 22d, 21d, 25d),
            new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 11d, 22d, 21d, 25d),
            new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 11d, 22d, 21d, 25d),
            new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.Ellipse, 39d, 13d, 6d, 5d),
            new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.Ellipse, 39d, 24d, 6d, 5d),
            new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.Ellipse, 34d, 18d, 6d, 6d),
            new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.Ellipse, 44d, 18d, 6d, 6d),
            new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 64d, 17d, 6d, 6d),
            new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 59d, 25d, 6d, 7d),
            new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 59d, 8d, 6d, 6d),
            new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 54d, 15d, 6d, 6d),
            new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 28d, 49d, 5d, 5d),
            new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 34d, 47d, 5d, 5d),
            new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 39d, 45d, 5d, 5d)
        ]);
}
