using GWGUI.Emulation;

namespace GWGUI.App.Input;

internal static class EmulationShortcutMap
{
    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> KeyboardMap(
        IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return new Dictionary<EmulationKey, EmulationKey>();
        var result = new Dictionary<EmulationKey, EmulationKey>();
        foreach (var mapping in mappings)
            if (Enum.TryParse<EmulationKey>(mapping.Key, true, out var amigaKey) && mapping.Value != EmulationKey.Unknown)
                result[mapping.Value] = amigaKey;
        return result;
    }

    internal static IReadOnlyList<KeyboardShortcutBinding> KeyboardShortcuts(
        IReadOnlyDictionary<string, string>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return [];
        var result = new List<KeyboardShortcutBinding>();
        foreach (var mapping in mappings)
        {
            if (Enum.TryParse<EmulationKey>(mapping.Key, true, out var amigaKey) &&
                KeyboardChord.TryParse(mapping.Value, out var chord))
                result.Add(new KeyboardShortcutBinding(chord, amigaKey));
        }
        return result;
    }

    internal static IReadOnlyList<GlobalShortcutBinding> GlobalShortcuts(
        IReadOnlyDictionary<string, string>? mappings) => mappings is null
        ? []
        : mappings.Select(mapping => KeyboardChord.TryParse(mapping.Value, out var chord)
                ? new GlobalShortcutBinding(mapping.Key, chord) : null)
            .Where(binding => binding is not null).Cast<GlobalShortcutBinding>().ToArray();
}

internal sealed record KeyboardShortcutBinding(KeyboardChord Chord, EmulationKey AmigaKey);
internal sealed record GlobalShortcutBinding(string Action, KeyboardChord Chord);
