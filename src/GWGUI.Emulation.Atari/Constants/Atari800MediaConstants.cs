namespace GWGUI.Emulation.Atari.Constants;

internal static class Atari800MediaConstants
{
    internal const string SystemOptionKey = "atari800_system";
    internal const string Atari5200SystemValue = "5200";
    internal const string CartridgeHeaderText = "CART";
    internal const int CartridgeHeaderLength = 4;
    internal const int MinimumCartridgeType = 1;

    internal static readonly IReadOnlySet<string> FloppyExtensions = Extensions(
        "xfd", "atr", "dcm", "atx", "m3u");
    internal static readonly IReadOnlySet<string> CassetteExtensions = Extensions("cas", "m3u");
    internal static readonly IReadOnlySet<string> CartridgeExtensions = Extensions("car", "bin", "rom", "a52");

    private static IReadOnlySet<string> Extensions(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}
