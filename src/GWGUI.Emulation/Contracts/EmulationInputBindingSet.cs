namespace GWGUI.Emulation.Contracts;

public sealed record EmulationInputBindingSet(
    IReadOnlyList<InputBindingDefinition> Definitions,
    IReadOnlyDictionary<string, string> Values,
    EmulationInputSource Sources,
    bool PrefixKeyboardSource = false);
