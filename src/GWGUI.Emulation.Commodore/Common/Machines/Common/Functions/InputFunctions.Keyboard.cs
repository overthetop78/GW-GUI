namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static class KeyboardBindingFunctions
{
    internal static IReadOnlySet<EmulationKey> Expand(
        IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>> momentary,
        IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>> toggles,
        ISet<EmulationKey> activeToggles,
        ISet<EmulationKey> previousSources
    )
    {
        foreach (var key in toggles.Keys)
            if (keys.Contains(key) && !previousSources.Contains(key))
                if (!activeToggles.Remove(key))
                    activeToggles.Add(key);
        previousSources.Clear();
        foreach (var key in toggles.Keys)
            if (keys.Contains(key))
                previousSources.Add(key);
        var expanded = new HashSet<EmulationKey>(keys.Where(key => !momentary.ContainsKey(key) && !toggles.ContainsKey(key)));
        foreach (var key in keys)
            if (momentary.TryGetValue(key, out var chord))
                expanded.UnionWith(chord);
        foreach (var key in activeToggles)
            if (toggles.TryGetValue(key, out var chord))
                expanded.UnionWith(chord);
        return expanded;
    }

    internal static InputConfiguration Normalize(InputConfiguration? input, IReadOnlyDictionary<EmulationKey, EmulationKey> defaults, IReadOnlyDictionary<EmulationKey, EmulationKey> legacyTargets)
    {
        input ??= new InputConfiguration();
        var result = defaults.ToDictionary(item => item.Key.ToString(), item => item.Value.ToString(), StringComparer.Ordinal);
        var source = input.KeyboardBindings ?? input.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal);
        if (source is not null)
            foreach (var item in source)
            {
                var target = Enum.TryParse<EmulationKey>(item.Key, true, out var key) ? legacyTargets.GetValueOrDefault(key, key).ToString() : item.Key;
                if (target != item.Key && source.ContainsKey(target)) continue;
                result[target] = item.Value;
            }
        var mappings = result.Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _)).ToDictionary(item => item.Key, item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal);
        return input with { KeyboardBindings = result, KeyboardMappings = mappings };
    }
}
