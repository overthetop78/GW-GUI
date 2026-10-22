using GWGUI.Emulation;
namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Dictionaries;
internal static class KeyboardDictionary
{
    private static readonly IReadOnlyList<EmulationKey> SpectrumKeys = Array.AsReadOnly(new[] { EmulationKey.SinclairSymbolShift });
    private static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> SpectrumDefaults = new Dictionary<EmulationKey, EmulationKey>
    {
        [EmulationKey.SinclairSymbolShift] = EmulationKey.LeftControl
    };
    private static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> NoDefaults = new Dictionary<EmulationKey, EmulationKey>();
    internal static IReadOnlyList<EmulationKey> Keys(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.Spectrum ? SpectrumKeys : [];
    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> Defaults(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.Spectrum ? SpectrumDefaults : NoDefaults;
    internal static IReadOnlyList<InputBindingDefinition> Definitions(MachineModel model) => Keys(model).Select(key => new InputBindingDefinition(key.ToString(),
        InputMappingConstants.KeyboardResourcePrefix + key,
        Defaults(model).TryGetValue(key, out var physical) ? physical.ToString() : string.Empty)).ToArray();
    internal static IReadOnlyDictionary<string, string> Bindings(InputConfiguration input, MachineModel model)
        => EmulationSdk.InputProcessing.ResolveKeyboardBindings(Definitions(model), input.Keyboard);

    internal static IReadOnlySet<EmulationKey> Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model) => EmulationSdk.InputProcessing.MapKeyboard(snapshot, Bindings(input, model));
}
