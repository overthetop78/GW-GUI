namespace GWGUI.Emulation.Nintendo.Emulators.Citra.Functions;

internal static class CitraOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        // The Citra option catalogue is supplied by the core at runtime; keep
        // the configured values opaque until that catalogue has been loaded.
        return configuration with
        {
            Options = new Dictionary<string, string>(
                configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        };
    }
}

