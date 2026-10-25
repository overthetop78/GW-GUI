using CommonControllerFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Wenquxing.Common.Machines.ElectronicDictionaries.Dictionaries;
internal static class KeyboardDictionary
{
    private static readonly IReadOnlyDictionary<EmulationKey, string> Labels = new Dictionary<EmulationKey, string>
    {
        [EmulationKey.WenquxingDictionary] = "DICT",
        [EmulationKey.WenquxingCard] = "CARD",
        [EmulationKey.WenquxingCalculator] = "CALC",
        [EmulationKey.WenquxingMemo] = "MEMO",
        [EmulationKey.WenquxingData] = "DATA",
        [EmulationKey.WenquxingClock] = "TIME",
        [EmulationKey.WenquxingNetwork] = "NET",
        [EmulationKey.WenquxingGame] = "GAME",
        [EmulationKey.WenquxingLexicon] = "LEX",
        [EmulationKey.WenquxingStudy] = "STUDY",
        [EmulationKey.WenquxingPower] = "ON",
        [EmulationKey.WenquxingInputMethod] = "IME",
        [EmulationKey.WenquxingHelp] = "HELP"
    };
    private static IReadOnlyDictionary<EmulationKey, EmulationKey> Defaults(MachineModel model)
    {
        if (!Enum.IsDefined(model))
            throw new ArgumentOutOfRangeException(nameof(model));
        var defaults = model == MachineModel.NC3000 ? new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.WenquxingGame] = EmulationKey.F5,
            [EmulationKey.WenquxingCalculator] = EmulationKey.F6,
            [EmulationKey.WenquxingClock] = EmulationKey.F7,
            [EmulationKey.WenquxingDictionary] = EmulationKey.F9,
            [EmulationKey.WenquxingLexicon] = EmulationKey.F10,
            [EmulationKey.WenquxingStudy] = EmulationKey.F11
        } : new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.WenquxingDictionary] = EmulationKey.F5,
            [EmulationKey.WenquxingCard] = EmulationKey.F6,
            [EmulationKey.WenquxingCalculator] = EmulationKey.F7,
            [EmulationKey.WenquxingMemo] = EmulationKey.F8,
            [EmulationKey.WenquxingData] = EmulationKey.F9,
            [EmulationKey.WenquxingClock] = EmulationKey.F10,
            [EmulationKey.WenquxingNetwork] = EmulationKey.F11
        };
        defaults[EmulationKey.WenquxingPower] = model == MachineModel.NC1020 ? EmulationKey.Delete : EmulationKey.F12;
        defaults[EmulationKey.WenquxingInputMethod] = EmulationKey.Backslash;
        defaults[EmulationKey.WenquxingHelp] = EmulationKey.LeftBracket;
        return defaults;
    }

    internal static IReadOnlyList<InputBindingDefinition> Definitions(MachineModel model) => Defaults(model).Select(pair => new InputBindingDefinition(pair.Key.ToString(), string.Empty, pair.Value.ToString(), Labels[pair.Key])).ToArray();
    internal static IReadOnlyDictionary<string, string> Bindings(InputConfiguration input, MachineModel model)
    {
        var bindings = Definitions(model).ToDictionary(definition => definition.Id, definition => definition.DefaultBinding, StringComparer.Ordinal);
        foreach (var pair in input.KeyboardBindings ?? new Dictionary<string, string>())
        {
            if (!bindings.ContainsKey(pair.Key) || pair.Value is null)
                throw new ArgumentException(nameof(input));
            var physical = pair.Value.StartsWith("Keyboard:", StringComparison.OrdinalIgnoreCase) ? pair.Value["Keyboard:".Length..] : pair.Value;
            if (!CommonControllerFunctions.IsValidBinding(physical))
                throw new ArgumentException(nameof(input));
            bindings[pair.Key] = physical;
        }

        return bindings;
    }

    internal static IReadOnlySet<EmulationKey> Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model) => EmulationSdk.InputProcessing.MapKeyboard(snapshot, Bindings(input, model));
}
