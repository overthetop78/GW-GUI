namespace GWGUI.Emulation.Sega.Emulators.BlueMSX.Constants;

internal static class MachineOptionConstants
{
    internal const string SystemOption = "bluemsx_msxtype";
    internal static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> All { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            [ModelConstants.Sg1000] = new Dictionary<string, string> { [SystemOption] = "SEGA - SG-1000" },
            [ModelConstants.Sc3000] = new Dictionary<string, string> { [SystemOption] = "SEGA - SC-3000" },
            [ModelConstants.Sf7000] = new Dictionary<string, string> { [SystemOption] = "SEGA - SF-7000" },
        };
}
