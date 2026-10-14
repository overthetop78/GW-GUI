namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class Pcsx2OptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var (key, aliases) in Dictionaries.OptionCatalog.LegacyValues)
            if (options.TryGetValue(key, out var value) && aliases.TryGetValue(value, out var current)) options[key] = current;
        return configuration with { Options = options };
    }
}


