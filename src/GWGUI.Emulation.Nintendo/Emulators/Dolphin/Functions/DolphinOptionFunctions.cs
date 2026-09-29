namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Functions;

internal static class DolphinOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        // The Dolphin option catalogue is supplied by the core at runtime; keep
        // the configured values opaque until that catalogue has been loaded.
        return configuration with
        {
            Options = new Dictionary<string, string>(
                configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        };
    }
}
