namespace GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Functions;

internal static class BeetleVbOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        // Beetle VB exposes its own option catalogue at runtime. Passing
        // option names from another core would make it reject the machine,
        // so only options explicitly returned by this core are forwarded.
        return configuration with
        {
            Options = new Dictionary<string, string>(
                configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        };
    }
}

