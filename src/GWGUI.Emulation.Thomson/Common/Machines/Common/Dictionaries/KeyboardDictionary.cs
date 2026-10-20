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
    private static readonly IReadOnlyDictionary<EmulationKey, string> Labels = new Dictionary<EmulationKey, string>
    {
        [EmulationKey.ThomsonStop] = "STOP",
        [EmulationKey.ThomsonAccent] = "ACC",
        [EmulationKey.ThomsonRaz] = "RAZ",
        [EmulationKey.ThomsonBasic] = "BASIC"
    };
    internal static IReadOnlyList<EmulationKey> Keys(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.Mo 
        ? [EmulationKey.ThomsonStop, EmulationKey.ThomsonAccent, EmulationKey.ThomsonRaz, EmulationKey.ThomsonBasic] 
        : [EmulationKey.ThomsonStop, EmulationKey.ThomsonAccent, EmulationKey.ThomsonRaz];
    internal static IReadOnlyList<InputBindingDefinition> Definitions(MachineModel model) => Keys(model).Select(key => new InputBindingDefinition(key.ToString(), string.Empty, Defaults[key].ToString(), Labels[key])).ToArray();
    internal static IReadOnlyDictionary<string, string> Bindings(InputConfiguration input, MachineModel model)
        => EmulationInputMappingFunctions.ResolveKeyboardBindings(Definitions(model), input.Keyboard);

    internal static IReadOnlySet<EmulationKey> Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model) => EmulationInputMappingFunctions.MapKeyboard(snapshot, Bindings(input, model));
}
