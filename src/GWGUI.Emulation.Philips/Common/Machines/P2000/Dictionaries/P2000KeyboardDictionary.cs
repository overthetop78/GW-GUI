namespace GWGUI.Emulation.Philips.Common.Machines.P2000.Dictionaries;

internal static class P2000KeyboardDictionary
{
    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> Defaults { get; } =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.PhilipsP2000Start] = EmulationKey.F1,
            [EmulationKey.PhilipsP2000Stop] = EmulationKey.F2,
            [EmulationKey.PhilipsP2000Zoek] = EmulationKey.F3,
            [EmulationKey.PhilipsP2000Code] = EmulationKey.Backquote,
            [EmulationKey.PhilipsP2000DoubleZero] = EmulationKey.F4,
            [EmulationKey.PhilipsP2000ClearLine] = EmulationKey.F5
        };

    internal static IReadOnlyList<InputBindingDefinition> Definitions { get; } = Defaults.Select(pair => new InputBindingDefinition(pair.Key.ToString(), InputMappingConstants.KeyboardResourcePrefix + pair.Key, pair.Value.ToString())).ToArray();

    internal static IReadOnlySet<EmulationKey> Apply(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string>? configured)
    {
        var result = new HashSet<EmulationKey>(snapshot.Keys);
        var controller = snapshot.Controllers.Count > 0 ? snapshot.Controllers[0] : EmulationControllerState.Empty;
        // Default host keys do not remain immutable aliases after reassignment.
        foreach (var nativeAlias in Defaults.Values) result.Remove(nativeAlias);
        foreach (var binding in Definitions)
        {
            var source = configured?.GetValueOrDefault(binding.Id, binding.DefaultBinding) ?? binding.DefaultBinding;
            if (!string.IsNullOrWhiteSpace(source) && EmulationInputMappingFunctions.InputSourcePressure(source, controller, snapshot) > .5f)
                result.Add(Enum.Parse<EmulationKey>(binding.Id));
        }
        return result;
    }

    internal static void Validate(IReadOnlyDictionary<string, string>? configured)
    {
        foreach (var pair in configured ?? new Dictionary<string, string>())
            if (!Definitions.Any(definition => definition.Id == pair.Key)
                || (!string.IsNullOrEmpty(pair.Value) && !TrySource(pair.Value, out _) && !EmulationKeyboardBindingFunctions.IsValid(pair.Value)))
                throw new ArgumentException(nameof(configured));
    }

    private static bool TrySource(string source, out EmulationKey key)
    {
        if (source.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase)) source = source[InputMappingConstants.KeyboardPrefix.Length..];
        return Enum.TryParse(source, out key) && Enum.IsDefined(key) && key != EmulationKey.Unknown && !Defaults.ContainsKey(key);
    }
}
