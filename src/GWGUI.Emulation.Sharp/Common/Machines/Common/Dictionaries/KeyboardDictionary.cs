namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Dictionaries;
internal static class KeyboardDictionary
{
    private static readonly IReadOnlyList<EmulationKey> X68000Keys = Array.AsReadOnly(new[] { EmulationKey.Help,
        EmulationKey.Undo,
        EmulationKey.Break,
        EmulationKey.SharpXf1,
        EmulationKey.SharpXf2,
        EmulationKey.SharpXf3,
        EmulationKey.SharpXf4,
        EmulationKey.SharpXf5,
        EmulationKey.SharpOption1,
        EmulationKey.SharpOption2,
        EmulationKey.SharpSymbol,
        EmulationKey.SharpRegister,
        EmulationKey.SharpKana,
        EmulationKey.SharpRoman,
        EmulationKey.SharpCode,
        EmulationKey.SharpHiragana,
        EmulationKey.SharpFullWidth,
        EmulationKey.SharpKeypadEquals });
    private static readonly IReadOnlyList<EmulationKey> X1Keys = Array.AsReadOnly(new[] { EmulationKey.Break, EmulationKey.SharpGraph, EmulationKey.SharpCopy, EmulationKey.SharpKeypadEquals });
    private static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> X68000Defaults = new Dictionary<EmulationKey, EmulationKey>
    {
        [EmulationKey.Help] = EmulationKey.Insert,
        [EmulationKey.Undo] = EmulationKey.Home,
        [EmulationKey.SharpOption1] = EmulationKey.LeftAlt,
        [EmulationKey.SharpOption2] = EmulationKey.RightAlt
    };
    private static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> X1Defaults = new Dictionary<EmulationKey, EmulationKey>
    {
        [EmulationKey.SharpGraph] = EmulationKey.LeftAlt
    };
    internal static IReadOnlyList<EmulationKey> Keys(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.X68000 ? X68000Keys : X1Keys;
    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> Defaults(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.X68000 ? X68000Defaults : X1Defaults;
    internal static IReadOnlyList<InputBindingDefinition> Definitions(MachineModel model) => Keys(model).Select(key => new InputBindingDefinition(key.ToString(),
        InputMappingConstants.KeyboardResourcePrefix + key,
        Defaults(model).TryGetValue(key, out var physical) ? physical.ToString() : string.Empty)).ToArray();
    internal static IReadOnlyDictionary<string, string> Bindings(InputConfiguration input, MachineModel model)
    {
        var bindings = Definitions(model).ToDictionary(definition => definition.Id, definition => definition.DefaultBinding, StringComparer.Ordinal);
        foreach (var pair in input.Keyboard ?? new Dictionary<string, string>())
        {
            if (!bindings.ContainsKey(pair.Key) || !IsPhysicalKey(pair.Value))
                throw new ArgumentException(nameof(input));
            bindings[pair.Key] = pair.Value;
        }

        return bindings;
    }

    internal static IReadOnlySet<EmulationKey> Apply(IReadOnlySet<EmulationKey> keys, InputConfiguration input, MachineModel model)
    {
        var mappings = Bindings(input, model).Where(pair => Enum.TryParse<EmulationKey>(pair.Value, out _)).ToDictionary(pair => pair.Key, pair => Enum.Parse<EmulationKey>(pair.Value), StringComparer.Ordinal);
        return EmulationInputMappingFunctions.MapKeyboard(keys, mappings);
    }

    private static bool IsPhysicalKey(string value) => EmulationKeyboardBindingFunctions.IsValid(value);
}