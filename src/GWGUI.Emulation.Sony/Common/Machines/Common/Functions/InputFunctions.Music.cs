namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> BeatmaniaDefinitions(int keyCount) =>
    [
        .. Enumerable.Range(1, keyCount).Select(key => new InputBindingDefinition("Beatmania.Key" + key, string.Empty, string.Empty, key.ToString())),
        new("Beatmania.ScratchLeft", "Emulation.Sony.Input.Music.ScratchLeft", string.Empty) { OppositeDirectionId = "Beatmania.ScratchRight" },
        new("Beatmania.ScratchRight", "Emulation.Sony.Input.Music.ScratchRight", string.Empty) { OppositeDirectionId = "Beatmania.ScratchLeft" },
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> PopnDefinitions() =>
    [
        .. new[] { "⚪", "🟡", "🟢", "🔵", "🔴", "🔵", "🟢", "🟡", "⚪" }
            .Select((color, index) => new InputBindingDefinition("Popn." + (index + 1), string.Empty, string.Empty, (index + 1) + " " + color)),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> TaikoDefinitions() =>
    [
        new("Taiko.LeftHead", "Emulation.Sony.Input.Taiko.LeftHead", string.Empty),
        new("Taiko.RightHead", "Emulation.Sony.Input.Taiko.RightHead", string.Empty),
        new("Taiko.LeftRim", "Emulation.Sony.Input.Taiko.LeftRim", string.Empty),
        new("Taiko.RightRim", "Emulation.Sony.Input.Taiko.RightRim", string.Empty),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> GuitarFreaksDefinitions() =>
    [
        .. GuitarDefinitions().Where(definition => definition.Id is "Guitar.Red" or "Guitar.Green" or "Guitar.Blue" or "Guitar.Tilt"
            or EmulationControllerCommandIds.Start or EmulationControllerCommandIds.Select),
        new("Guitar.Pick", "Emulation.Sony.Input.Guitar.Pick", string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> KonamiDdrDefinitions() =>
    [
        Definition(EmulationControllerCommandIds.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        Definition(EmulationControllerCommandIds.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        Definition(EmulationControllerCommandIds.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        Definition(EmulationControllerCommandIds.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        Definition(EmulationControllerCommandIds.B, InputSettingsFunctionsConstants.ControllerCross, string.Empty),
        Definition(EmulationControllerCommandIds.A, InputSettingsFunctionsConstants.ControllerCircle, string.Empty),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty)
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? MusicVisualCommands(ControllerType type)
    {
        if (type == ControllerType.TaikoDrum) return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.PrimaryAction] = "Taiko.LeftHead",
            [EmulationControllerVisualControl.SecondaryAction] = "Taiko.RightHead",
            [EmulationControllerVisualControl.TertiaryAction] = "Taiko.LeftRim",
            [EmulationControllerVisualControl.QuaternaryAction] = "Taiko.RightRim",
            [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
            [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start
        };
        if (type is not (ControllerType.Beatmania or ControllerType.BeatmaniaIidx or ControllerType.PopnMusic)) return null;
        var count = type == ControllerType.Beatmania ? 5 : type == ControllerType.BeatmaniaIidx ? 7 : 9;
        var prefix = type == ControllerType.PopnMusic ? "Popn." : "Beatmania.Key";
        var commands = Enumerable.Range(1, count).ToDictionary(key => Enum.Parse<EmulationControllerVisualControl>("Key" + key), key => prefix + key);
        commands[EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select;
        commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
        if (type != ControllerType.PopnMusic)
        {
            commands[EmulationControllerVisualControl.RotaryLeft] = "Beatmania.ScratchLeft";
            commands[EmulationControllerVisualControl.RotaryRight] = "Beatmania.ScratchRight";
        }
        return commands;
    }
}
