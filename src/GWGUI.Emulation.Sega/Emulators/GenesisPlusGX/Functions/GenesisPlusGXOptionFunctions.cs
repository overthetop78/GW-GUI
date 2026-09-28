namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Functions;

internal static class GenesisPlusGXOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        // Genesis Plus GX exposes its own option catalogue at runtime. Passing
        // the copied CPC option names would make the core reject the machine,
        // so only options explicitly returned by this core are forwarded.
        return configuration with
        {
            Options = new Dictionary<string, string>(
                configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        };
    }

    internal static IReadOnlyDictionary<string, string> FilterToCoreOptions(
        IReadOnlyDictionary<string, string>? options, IReadOnlyList<CoreOption> catalog)
    {
        var keys = catalog.Select(option => option.Key)
            .ToHashSet(StringComparer.Ordinal);
        return (options ?? new Dictionary<string, string>())
            .Where(option => keys.Contains(option.Key))
            .ToDictionary(option => option.Key, option => option.Value, StringComparer.Ordinal);
    }
}
