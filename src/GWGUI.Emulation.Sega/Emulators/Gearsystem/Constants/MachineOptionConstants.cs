namespace GWGUI.Emulation.Sega.Emulators.Gearsystem.Constants;

internal static class MachineOptionConstants
{
    internal const string SystemOption = "gearsystem_system";
    internal static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> All { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            [ModelConstants.Sg1000] = new Dictionary<string, string> { [SystemOption] = "SG-1000 / Multivision" },
            [ModelConstants.MarkIII] = new Dictionary<string, string> { [SystemOption] = "Master System / Mark III" },
            [ModelConstants.MasterSystem] = new Dictionary<string, string> { [SystemOption] = "Master System / Mark III" },
            [ModelConstants.GameGear] = new Dictionary<string, string> { [SystemOption] = "Game Gear (2 ASIC)" },
        };
}
