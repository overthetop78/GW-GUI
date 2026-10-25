namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGXWide.Constants;

internal static class MachineOptionConstants
{
    internal const string SystemOption = "genesis_plus_gx_wide_system_hw";
    internal static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> All { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            [ModelConstants.Sg1000] = new Dictionary<string, string> { [SystemOption] = "sg-1000" },
            [ModelConstants.Sc3000] = new Dictionary<string, string> { [SystemOption] = "sg-1000" },
            [ModelConstants.MarkIII] = new Dictionary<string, string> { [SystemOption] = "mark-III" },
            [ModelConstants.MasterSystem] = new Dictionary<string, string> { [SystemOption] = "master system" },
            [ModelConstants.GameGear] = new Dictionary<string, string> { [SystemOption] = "game gear" },
            [ModelConstants.MegaDrive] = new Dictionary<string, string> { [SystemOption] = "mega drive / genesis" },
            [ModelConstants.MegaCd] = new Dictionary<string, string> { [SystemOption] = "mega drive / genesis" },
        };
}
