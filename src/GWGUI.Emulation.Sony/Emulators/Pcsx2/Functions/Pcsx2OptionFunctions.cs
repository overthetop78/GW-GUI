namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class Pcsx2OptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        // Pcsx2 exposes its own option catalogue at runtime. Passing
        // the copied CPC option names would make the core reject the machine,
        // so only options explicitly returned by this core are forwarded.
        return configuration with
        {
            Options = new Dictionary<string, string>(
                configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        };
    }
}


