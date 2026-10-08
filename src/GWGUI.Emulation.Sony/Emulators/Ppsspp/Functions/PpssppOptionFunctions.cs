namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;

internal static class PpssppOptionFunctions
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


