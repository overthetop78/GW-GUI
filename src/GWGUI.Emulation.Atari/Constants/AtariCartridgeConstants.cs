namespace GWGUI.Emulation.Atari.Constants;

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

    internal static readonly IReadOnlySet<AtariEmulator> CartridgeCores =
        new HashSet<AtariEmulator>
        {
            AtariEmulator.Stella,
            AtariEmulator.ProSystem,
            AtariEmulator.BeetleLynx,
            AtariEmulator.VirtualJaguar
        };

    internal static readonly IReadOnlyDictionary<AtariEmulator, IReadOnlySet<string>> Extensions =
        new Dictionary<AtariEmulator, IReadOnlySet<string>>
        {
            [AtariEmulator.Stella] = Values("a26", "bin"),
            [AtariEmulator.ProSystem] = Values("a78", "bin", "cdf"),
            [AtariEmulator.BeetleLynx] = Values("lnx", "lyx", "bll", "o"),
            [AtariEmulator.VirtualJaguar] = Values("j64", "jag", "rom", "abs", "cof", "bin", "prg")
        };

    private static IReadOnlySet<string> Values(params string[] extensions) =>
        new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
}
