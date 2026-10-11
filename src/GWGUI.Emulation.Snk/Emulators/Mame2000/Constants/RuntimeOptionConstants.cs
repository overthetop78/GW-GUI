namespace GWGUI.Emulation.Snk.Emulators.Mame2000.Constants;

internal static class RuntimeOptionConstants
{
    internal const string Frameskip = "mame2000-frameskip";
    internal const string FrameskipThreshold = "mame2000-frameskip_threshold";
    internal const string FrameskipInterval = "mame2000-frameskip_interval";
    internal const string SkipDisclaimer = "mame2000-skip_disclaimer";
    internal const string ShowGameInfo = "mame2000-show_gameinfo";
    internal const string SampleRate = "mame2000-sample_rate";
    internal const string Stereo = "mame2000-stereo";
    internal const string QsoundOutputFilter = "mame2000-qsound_output_filter";
    internal const string Disabled = "disabled";
    internal const string Threshold = "threshold";
    internal const string SystemArchive = "neogeo.zip";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip" });
}
