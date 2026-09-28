namespace GWGUI.Emulation.Sega.Emulators.Yabause.Functions;

internal static class YabauseOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        // Yabause exposes its own option catalogue at runtime. Passing
        // the copied CPC option names would make the core reject the machine,
        // so only options explicitly returned by this core are forwarded.
        return configuration with
        {
            Options = new Dictionary<string, string>(
                configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        };
    }
}
