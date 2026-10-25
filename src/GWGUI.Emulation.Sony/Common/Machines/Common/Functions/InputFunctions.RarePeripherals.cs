namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> KidsStationDefinitions() => PlayStationButtonDefinitions()
        .Where(definition => definition.Id is EmulationControllerCommandIds.A or EmulationControllerCommandIds.B
            or EmulationControllerCommandIds.X or EmulationControllerCommandIds.Y).ToArray();

    private static InputBindingDefinition RareDefinition(string id) => new(id, "Emulation.Sony.Input." + id, string.Empty);

    private static IReadOnlyList<InputBindingDefinition> MaestroDefinitions() =>
    [
        RareDefinition("Maestro.Button1"), RareDefinition("Maestro.Button2"), RareDefinition("Maestro.Swing")
    ];

    private static IReadOnlyList<InputBindingDefinition> TambourineDefinitions() =>
    [
        RareDefinition("Tambourine.LeftButton"), RareDefinition("Tambourine.RightButton"),
        RareDefinition("Tambourine.Hit"), RareDefinition("Tambourine.Shake"),
        RareDefinition("Tambourine.MoveLeft") with { OppositeDirectionId = "Tambourine.MoveRight" },
        RareDefinition("Tambourine.MoveRight") with { OppositeDirectionId = "Tambourine.MoveLeft" },
        RareDefinition("Tambourine.MoveUp") with { OppositeDirectionId = "Tambourine.MoveDown" },
        RareDefinition("Tambourine.MoveDown") with { OppositeDirectionId = "Tambourine.MoveUp" }
    ];

    // Physical controls are prepared independently of any emulator's USB implementation.
    // Pressure-sensitive A/B are represented as presses; no native pressure protocol is implied.
    private static IReadOnlyList<InputBindingDefinition> FlightStick2Definitions() =>
    [
        .. JetDefinitions().Where(definition => definition.Id.StartsWith("Jet.", StringComparison.Ordinal)),
        .. AnalogStickDefinitions().Where(definition => definition.Id.StartsWith("RightStick", StringComparison.Ordinal)),
        RareDefinition("Flight.RudderLeft") with { OppositeDirectionId = "Flight.RudderRight" },
        RareDefinition("Flight.RudderRight") with { OppositeDirectionId = "Flight.RudderLeft" },
        RareDefinition("Flight.Knob1Left") with { OppositeDirectionId = "Flight.Knob1Right" },
        RareDefinition("Flight.Knob1Right") with { OppositeDirectionId = "Flight.Knob1Left" },
        RareDefinition("Flight.Knob2Left") with { OppositeDirectionId = "Flight.Knob2Right" },
        RareDefinition("Flight.Knob2Right") with { OppositeDirectionId = "Flight.Knob2Left" },
        RareDefinition("Flight.HatPress"),
        .. new[] { "TRIGGER", "FIRE C", "LAUNCH", "A", "B", "SW 1", "D", "ST", "M1", "M2", "M3" }
            .Select(label => new InputBindingDefinition("Flight." + label.Replace(" ", string.Empty), string.Empty, string.Empty, label)),
        .. Enumerable.Range(1, 2).SelectMany(pad => new[] { ("Up", "↑"), ("Down", "↓"), ("Left", "←"), ("Right", "→") }
            .Select(direction => new InputBindingDefinition($"Flight.DPad{pad}{direction.Item1}", string.Empty, string.Empty, $"D-PAD {pad} {direction.Item2}"))),
        .. new[] { ("Left", "←"), ("Middle", "●"), ("Right", "→") }
            .Select(direction => new InputBindingDefinition("Flight.DPad3" + direction.Item1, string.Empty, string.Empty, "D-PAD 3 " + direction.Item2))
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? RarePeripheralVisualCommands(PeripheralCategory type)
    {
        if (type == PeripheralCategory.KidsStation) return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
            [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
            [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X
        };
        if (type == PeripheralCategory.MaestroBaton) return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.PrimaryAction] = "Maestro.Button1",
            [EmulationControllerVisualControl.SecondaryAction] = "Maestro.Button2"
        };
        if (type == PeripheralCategory.Tambourine) return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.PrimaryAction] = "Tambourine.LeftButton",
            [EmulationControllerVisualControl.SecondaryAction] = "Tambourine.RightButton",
            [EmulationControllerVisualControl.TertiaryAction] = "Tambourine.Hit"
        };
        if (type != PeripheralCategory.UsbFlightStick) return null;
        return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = "Flight.DPad1Up",
            [EmulationControllerVisualControl.DirectionDown] = "Flight.DPad1Down",
            [EmulationControllerVisualControl.DirectionLeft] = "Flight.DPad1Left",
            [EmulationControllerVisualControl.DirectionRight] = "Flight.DPad1Right",
            [EmulationControllerVisualControl.CUp] = "Flight.DPad2Up",
            [EmulationControllerVisualControl.CDown] = "Flight.DPad2Down",
            [EmulationControllerVisualControl.CLeft] = "Flight.DPad2Left",
            [EmulationControllerVisualControl.CRight] = "Flight.DPad2Right",
            [EmulationControllerVisualControl.FaceX] = "Flight.DPad3Left",
            [EmulationControllerVisualControl.FaceY] = "Flight.DPad3Middle",
            [EmulationControllerVisualControl.FaceZ] = "Flight.DPad3Right",
            [EmulationControllerVisualControl.FaceA] = "Flight.A",
            [EmulationControllerVisualControl.FaceB] = "Flight.B",
            [EmulationControllerVisualControl.FaceC] = "Flight.FIREC",
            [EmulationControllerVisualControl.FaceD] = "Flight.D",
            [EmulationControllerVisualControl.PrimaryAction] = "Flight.LAUNCH",
            [EmulationControllerVisualControl.RightTrigger] = "Flight.TRIGGER",
            [EmulationControllerVisualControl.Guide] = "Flight.SW1",
            [EmulationControllerVisualControl.Start] = "Flight.ST",
            [EmulationControllerVisualControl.Key1] = "Flight.M1",
            [EmulationControllerVisualControl.Key2] = "Flight.M2",
            [EmulationControllerVisualControl.Key3] = "Flight.M3",
            [EmulationControllerVisualControl.StickUp] = "Jet.PitchUp",
            [EmulationControllerVisualControl.StickDown] = "Jet.PitchDown",
            [EmulationControllerVisualControl.StickLeft] = "Jet.RollLeft",
            [EmulationControllerVisualControl.StickRight] = "Jet.RollRight",
            [EmulationControllerVisualControl.RightStickUp] = InputSettingsFunctionsConstants.RightStickUp,
            [EmulationControllerVisualControl.RightStickDown] = InputSettingsFunctionsConstants.RightStickDown,
            [EmulationControllerVisualControl.RightStickLeft] = InputSettingsFunctionsConstants.RightStickLeft,
            [EmulationControllerVisualControl.RightStickRight] = InputSettingsFunctionsConstants.RightStickRight,
            [EmulationControllerVisualControl.RightStick] = "Flight.HatPress",
            [EmulationControllerVisualControl.Accelerator] = "Jet.ThrottleIncrease",
            [EmulationControllerVisualControl.BrakePedal] = "Jet.ThrottleDecrease",
            [EmulationControllerVisualControl.RotaryLeft] = "Flight.RudderLeft",
            [EmulationControllerVisualControl.RotaryRight] = "Flight.RudderRight",
            [EmulationControllerVisualControl.DialLeft] = "Flight.Knob1Left",
            [EmulationControllerVisualControl.DialRight] = "Flight.Knob1Right",
            [EmulationControllerVisualControl.GearDown] = "Flight.Knob2Left",
            [EmulationControllerVisualControl.GearUp] = "Flight.Knob2Right"
        };
    }
}
