namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static readonly string[] InstrumentColors = ["Green", "Red", "Yellow", "Blue", "Orange"];
    private static readonly string[] PianoNotes = ["C", "C♯", "D", "D♯", "E", "F", "F♯", "G", "G♯", "A", "A♯", "B"];
    private static readonly string[] TetherDirections = ["Left", "Right", "Forward", "Backward", "Extend", "Retract"];

    private static InputBindingDefinition InstrumentPair(string id, string opposite) =>
        RareDefinition(id) with { OppositeDirectionId = opposite };

    private static IEnumerable<InputBindingDefinition> InstrumentMenuDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is EmulationControllerCommandIds.Up
            or EmulationControllerCommandIds.Down or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right
            or EmulationControllerCommandIds.A or EmulationControllerCommandIds.B or EmulationControllerCommandIds.X
            or EmulationControllerCommandIds.Y or EmulationControllerCommandIds.Select or EmulationControllerCommandIds.Start),
        new("Instrument.PS", string.Empty, string.Empty, "PS")
    ];

    private static IReadOnlyList<InputBindingDefinition> KeyboardManiaDefinitions() =>
    [
        .. Enumerable.Range(0, 24).Select(note => new InputBindingDefinition("Piano.Key" + (note + 1),
            string.Empty, string.Empty, PianoNotes[note % 12] + (3 + note / 12))),
        InstrumentPair("Piano.WheelUp", "Piano.WheelDown"), InstrumentPair("Piano.WheelDown", "Piano.WheelUp"),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> DrumManiaDefinitions() =>
    [
        .. new[] { "HiHat", "Snare", "HighTom", "LowTom", "Cymbal", "Kick" }.Select(pad => RareDefinition("Drum." + pad)),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> ParaParaDefinitions() =>
    [
        .. Enumerable.Range(1, 5).Select(sensor => RareDefinition("ParaPara.Sensor" + sensor)),
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is EmulationControllerCommandIds.Left
            or EmulationControllerCommandIds.Right or EmulationControllerCommandIds.Select or EmulationControllerCommandIds.Start)
    ];

    private static IReadOnlyList<InputBindingDefinition> RockBandGuitarDefinitions() =>
    [
        .. GuitarDefinitions(),
        .. InstrumentColors.Select(color => RareDefinition("Guitar.Solo" + color)),
        .. Enumerable.Range(1, 5).Select(position => RareDefinition("Guitar.Effect" + position)),
        .. InstrumentMenuDefinitions().Where(definition => definition.Id is EmulationControllerCommandIds.Up
            or EmulationControllerCommandIds.Down or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right or "Instrument.PS")
    ];

    private static IReadOnlyList<InputBindingDefinition> BandDrumDefinitions(bool guitarHero) =>
    [
        .. (guitarHero ? new[] { "Red", "Blue", "Green", "Yellow", "Orange" } : new[] { "Red", "Yellow", "Blue", "Green" })
            .Select(color => RareDefinition("Drum." + color)),
        RareDefinition("Drum.Kick"),
        .. InstrumentMenuDefinitions()
    ];

    private static IReadOnlyList<InputBindingDefinition> DjHeroDefinitions() =>
    [
        .. InstrumentMenuDefinitions(),
        .. new[] { "Green", "Red", "Blue" }.Select(color => RareDefinition("DJ." + color)),
        InstrumentPair("DJ.ScratchLeft", "DJ.ScratchRight"), InstrumentPair("DJ.ScratchRight", "DJ.ScratchLeft"),
        InstrumentPair("DJ.CrossfaderLeft", "DJ.CrossfaderRight"), InstrumentPair("DJ.CrossfaderRight", "DJ.CrossfaderLeft"),
        InstrumentPair("DJ.EffectLeft", "DJ.EffectRight"), InstrumentPair("DJ.EffectRight", "DJ.EffectLeft")
    ];

    private static IReadOnlyList<InputBindingDefinition> GameTrakDefinitions() =>
    [
        .. new[] { "Left", "Right" }.SelectMany(hand => TetherDirections.Select((direction, index) =>
            InstrumentPair("GameTrak." + hand + direction, "GameTrak." + hand + TetherDirections[index ^ 1]))),
        RareDefinition("GameTrak.Pedal")
    ];

    private static IReadOnlyList<InputBindingDefinition>? InstrumentDefinitions(PeripheralCategory type) => type switch
    {
        PeripheralCategory.KeyboardMania => KeyboardManiaDefinitions(),
        PeripheralCategory.DrumMania => DrumManiaDefinitions(),
        PeripheralCategory.ParaParaController => ParaParaDefinitions(),
        PeripheralCategory.RockBandGuitar => RockBandGuitarDefinitions(),
        PeripheralCategory.RockBandDrums => BandDrumDefinitions(false),
        PeripheralCategory.GuitarHeroDrums => BandDrumDefinitions(true),
        PeripheralCategory.DjHero => DjHeroDefinitions(),
        PeripheralCategory.GameTrak => GameTrakDefinitions(),
        _ => null
    };

    private static Dictionary<EmulationControllerVisualControl, string> InstrumentMenuVisualCommands() => new()
    {
        [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
        [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
        [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
        [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
        [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
        [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
        [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
        [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
        [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
        [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
        [EmulationControllerVisualControl.Guide] = "Instrument.PS"
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? InstrumentVisualCommands(PeripheralCategory type)
    {
        if (InstrumentDefinitions(type) is null) return null;
        var commands = InstrumentMenuVisualCommands();
        switch (type)
        {
            case PeripheralCategory.KeyboardMania:
                for (var key = 1; key <= 24; key++) commands[Enum.Parse<EmulationControllerVisualControl>("Key" + key)] = "Piano.Key" + key;
                commands[EmulationControllerVisualControl.RotaryLeft] = "Piano.WheelUp";
                commands[EmulationControllerVisualControl.RotaryRight] = "Piano.WheelDown";
                break;
            case PeripheralCategory.DrumMania:
                foreach (var pad in new[] { "HiHat", "Snare", "HighTom", "LowTom", "Cymbal", "Kick" })
                    commands[Enum.Parse<EmulationControllerVisualControl>("Drum" + pad)] = "Drum." + pad;
                break;
            case PeripheralCategory.ParaParaController:
                for (var sensor = 1; sensor <= 5; sensor++) commands[Enum.Parse<EmulationControllerVisualControl>("OpticalSensor" + sensor)] = "ParaPara.Sensor" + sensor;
                break;
            case PeripheralCategory.RockBandGuitar:
                foreach (var pair in GuitarVisualCommands()) commands[pair.Key] = pair.Value;
                foreach (var color in InstrumentColors) commands[Enum.Parse<EmulationControllerVisualControl>("SoloFret" + color)] = "Guitar.Solo" + color;
                for (var position = 1; position <= 5; position++) commands[Enum.Parse<EmulationControllerVisualControl>("EffectPosition" + position)] = "Guitar.Effect" + position;
                break;
            case PeripheralCategory.RockBandDrums:
            case PeripheralCategory.GuitarHeroDrums:
                foreach (var color in InstrumentColors) commands[Enum.Parse<EmulationControllerVisualControl>("Drum" + color)] = "Drum." + color;
                commands[EmulationControllerVisualControl.DrumKick] = "Drum.Kick";
                break;
            case PeripheralCategory.DjHero:
                commands[EmulationControllerVisualControl.FretGreen] = "DJ.Green";
                commands[EmulationControllerVisualControl.FretRed] = "DJ.Red";
                commands[EmulationControllerVisualControl.FretBlue] = "DJ.Blue";
                commands[EmulationControllerVisualControl.RotaryLeft] = "DJ.ScratchLeft";
                commands[EmulationControllerVisualControl.RotaryRight] = "DJ.ScratchRight";
                commands[EmulationControllerVisualControl.CrossfaderLeft] = "DJ.CrossfaderLeft";
                commands[EmulationControllerVisualControl.CrossfaderRight] = "DJ.CrossfaderRight";
                commands[EmulationControllerVisualControl.DialLeft] = "DJ.EffectLeft";
                commands[EmulationControllerVisualControl.DialRight] = "DJ.EffectRight";
                // Euphoria and the triangle menu button are the same hardware input.
                commands[EmulationControllerVisualControl.AnalogMode] = EmulationControllerCommandIds.X;
                break;
            case PeripheralCategory.GameTrak:
                foreach (var hand in new[] { "Left", "Right" })
                    foreach (var direction in TetherDirections)
                        commands[Enum.Parse<EmulationControllerVisualControl>(hand + "Tether" + direction)] = "GameTrak." + hand + direction;
                commands[EmulationControllerVisualControl.PrimaryAction] = "GameTrak.Pedal";
                break;
        }
        return commands;
    }
}
