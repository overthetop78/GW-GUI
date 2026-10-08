namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;

internal static class SwanStationOptionFunctions
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


