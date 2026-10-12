namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Dictionaries;

internal static class KeyboardDictionary
{
    private static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> Defaults = new Dictionary<EmulationKey, EmulationKey>
    {
        [EmulationKey.ThomsonStop] = EmulationKey.Tab,
        [EmulationKey.ThomsonAccent] = EmulationKey.Backspace,
        [EmulationKey.ThomsonRaz] = EmulationKey.LeftAlt,
        [EmulationKey.ThomsonBasic] = EmulationKey.RightShift
    };
    private static readonly IReadOnlyDictionary<EmulationKey, string> Labels = new Dictionary<EmulationKey, string> { [EmulationKey.ThomsonStop] = "STOP", [EmulationKey.ThomsonAccent] = "ACC", [EmulationKey.ThomsonRaz] = "RAZ", [EmulationKey.ThomsonBasic] = "BASIC" };

    internal static IReadOnlyList<EmulationKey> Keys(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.Mo
        ? [EmulationKey.ThomsonStop, EmulationKey.ThomsonAccent, EmulationKey.ThomsonRaz, EmulationKey.ThomsonBasic]
        : [EmulationKey.ThomsonStop, EmulationKey.ThomsonAccent, EmulationKey.ThomsonRaz];
    internal static IReadOnlyList<InputBindingDefinition> Definitions(MachineModel model) => Keys(model).Select(key => new InputBindingDefinition(key.ToString(), string.Empty, Defaults[key].ToString(), Labels[key])).ToArray();

    internal static IReadOnlyDictionary<string, string> Bindings(InputConfiguration input, MachineModel model)
    {
        var bindings = Definitions(model).ToDictionary(definition => definition.Id, definition => definition.DefaultBinding, StringComparer.Ordinal);
        foreach (var pair in input.Keyboard ?? new Dictionary<string, string>())
        {
            if (!bindings.ContainsKey(pair.Key) || !IsPhysicalKey(pair.Value)) throw new ArgumentException(nameof(input));
            bindings[pair.Key] = pair.Value;
        }
        return bindings;
    }

    internal static IReadOnlySet<EmulationKey> Apply(IReadOnlySet<EmulationKey> keys, InputConfiguration input, MachineModel model)
    {
        var mappings = Bindings(input, model).Where(pair => !string.IsNullOrEmpty(pair.Value)).ToDictionary(pair => pair.Key, pair => Enum.Parse<EmulationKey>(pair.Value), StringComparer.Ordinal);
        return EmulationInputMappingFunctions.MapKeyboard(keys, mappings);
    }

    private static bool IsPhysicalKey(string value) => string.IsNullOrEmpty(value) || Enum.TryParse<EmulationKey>(value, out var key) && Enum.IsDefined(key) && key != EmulationKey.Unknown;
}
