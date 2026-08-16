namespace GWGUI.Emulation.Atari;

internal static class AtariCartridgeConstants
{
    internal const string StellaRegionOptionKey = "stella_console";
    internal const string JaguarRegionOptionKey = "virtualjaguar_pal";
    internal const string AutomaticRegionValue = "auto";
    internal const string NtscRegionValue = "ntsc";
    internal const string PalRegionValue = "pal";
    internal const string SecamRegionValue = "secam";
    internal const string EnabledValue = "enabled";
    internal const string DisabledValue = "disabled";

    internal static readonly IReadOnlySet<AtariCoreKind> CartridgeCores =
        new HashSet<AtariCoreKind>
        {
            AtariCoreKind.Stella,
            AtariCoreKind.ProSystem,
            AtariCoreKind.BeetleLynx,
            AtariCoreKind.VirtualJaguar
        };

    internal static readonly IReadOnlyDictionary<AtariCoreKind, IReadOnlySet<string>> Extensions =
        new Dictionary<AtariCoreKind, IReadOnlySet<string>>
        {
            [AtariCoreKind.Stella] = Values("a26", "bin"),
            [AtariCoreKind.ProSystem] = Values("a78", "bin", "cdf"),
            [AtariCoreKind.BeetleLynx] = Values("lnx", "lyx", "bll", "o"),
            [AtariCoreKind.VirtualJaguar] = Values("j64", "jag", "rom", "abs", "cof", "bin", "prg")
        };

    private static IReadOnlySet<string> Values(params string[] extensions) =>
        new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
}
