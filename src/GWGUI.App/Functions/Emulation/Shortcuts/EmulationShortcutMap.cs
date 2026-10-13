using GWGUI.App.Contracts.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Emulation.Shortcuts;

internal static class EmulationShortcutMap
{
    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> KeyboardMap(IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return new Dictionary<EmulationKey, EmulationKey>();
        var result = new Dictionary<EmulationKey, EmulationKey>();
        foreach (var mapping in mappings)
            if (Enum.TryParse<EmulationKey>(mapping.Key, true, out var emulationKey) &&
                mapping.Value != EmulationKey.Unknown)
                result[mapping.Value] = emulationKey;
        return result;
    }

    internal static IReadOnlyList<KeyboardShortcutBinding> KeyboardShortcuts(IReadOnlyDictionary<string, string>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return [];
        var result = new List<KeyboardShortcutBinding>();
        foreach (var mapping in mappings)
        {
            if (Enum.TryParse<EmulationKey>(mapping.Key, true, out var emulationKey) &&
                KeyboardChordFunctions.TryParse(mapping.Value, out var chord) && !KeyboardChordFunctions.IsWindowsReserved(chord))
                result.Add(new KeyboardShortcutBinding(chord, emulationKey));
        }
        return result;
    }

    internal static IReadOnlyList<GlobalShortcutBinding> GlobalShortcuts(IReadOnlyDictionary<string, string>? mappings) => mappings is null
        ? []
        : mappings.Select(mapping =>
        {
            var hasKeyboard = InputBindingSyntax.TryGetKeyboardChord(mapping.Value, out var chord);
            if (hasKeyboard && KeyboardChordFunctions.IsWindowsReserved(chord)) return null;
            if (InputBindingSyntax.HasNonKeyboardSource(mapping.Value))
                return EmulationKeyboardBindingFunctions.IsValid(mapping.Value) ? new GlobalShortcutBinding(mapping.Key, chord, mapping.Value) : null;
            return hasKeyboard ? new GlobalShortcutBinding(mapping.Key, chord) : null;
        }).Where(binding => binding is not null).Cast<GlobalShortcutBinding>().ToArray();
}
