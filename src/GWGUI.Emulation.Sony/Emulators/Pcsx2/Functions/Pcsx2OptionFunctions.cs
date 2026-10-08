namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class Pcsx2OptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        return configuration with
        {
            Options = new Dictionary<string, string>(
                configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        };
    }
}


