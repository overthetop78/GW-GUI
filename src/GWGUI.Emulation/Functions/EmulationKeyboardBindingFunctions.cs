namespace GWGUI.Emulation.Functions;

public static class EmulationKeyboardBindingFunctions
{
    internal static IReadOnlyDictionary<string, EmulationKey> ToKeys(this IReadOnlyDictionary<string, string> values) =>
        values.Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _)).ToDictionary(item => item.Key,
            item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal);

    internal static IReadOnlyDictionary<string, string> ToStrings(IReadOnlyDictionary<string, EmulationKey>? values,
        IEqualityComparer<string>? emptyComparer = null) =>
        values?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal)
        ?? (emptyComparer is null ? new Dictionary<string, string>() : new Dictionary<string, string>(emptyComparer));

    public static bool IsValid(string? binding)
    {
        if (string.IsNullOrWhiteSpace(binding)) return true;
        var parts = binding.Split('+', StringSplitOptions.TrimEntries);
        return parts.All(part =>
        {
            if (part.StartsWith("Mouse:", StringComparison.OrdinalIgnoreCase))
                return part[6..].ToLowerInvariant() is "left" or "right" or "middle" or "xbutton1" or "xbutton2" or "wheelup" or "wheeldown" or "wheelleft" or "wheelright";
            if (part.StartsWith("Controller:", StringComparison.OrdinalIgnoreCase))
                return part.Length > "Controller:".Length && !string.IsNullOrWhiteSpace(part[(part.LastIndexOf(':') + 1)..]);
            var keyPart = part.StartsWith("Keyboard:", StringComparison.OrdinalIgnoreCase) ? part[9..] : part;
            return keyPart.ToLowerInvariant() is "ctrl" or "control" or "alt" or "shift" or "maj"
                || TryKey(keyPart, out _);
        });
    }

    public static bool IsPressed(string binding, IReadOnlySet<EmulationKey> keys) => binding.Split('+', StringSplitOptions.TrimEntries).All(part =>
    {
        var keyPart = part.StartsWith("Keyboard:", StringComparison.OrdinalIgnoreCase) ? part[9..] : part;
        return keyPart.ToLowerInvariant() switch
        {
            "ctrl" or "control" => keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl),
            "alt" => keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt),
            "shift" or "maj" => keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift),
            _ => TryKey(keyPart, out var key) && keys.Contains(key)
        };
    });

    private static bool TryKey(string part, out EmulationKey key)
    {
        part = part switch
        {
            "LeftCtrl" => nameof(EmulationKey.LeftControl), "RightCtrl" => nameof(EmulationKey.RightControl),
            "Enter" => nameof(EmulationKey.Return), "Back" => nameof(EmulationKey.Backspace),
            "Decimal" => nameof(EmulationKey.NumpadPeriod),
            _ => part
        };
        return Enum.TryParse(part, true, out key) && Enum.IsDefined(key) && key != EmulationKey.Unknown;
    }
}
