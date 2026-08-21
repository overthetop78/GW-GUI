namespace GWGUI.Emulation;

public sealed record EmulationInputBindingSet(
    IReadOnlyList<InputBindingDefinition> Definitions,
    IReadOnlyDictionary<string, string> Values,
    EmulationInputSource Sources,
    bool PrefixKeyboardSource = false);
