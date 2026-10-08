namespace GWGUI.Emulation.Sega.Emulators.BlastEm.Constants;

internal static class MachineOptionConstants
{
    internal const string SystemOption = "blastem_system_type";
    internal static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> All { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            [ModelConstants.MegaDrive] = new Dictionary<string, string> { [SystemOption] = "gen" },
        };
}
