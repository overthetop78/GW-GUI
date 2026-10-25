namespace GWGUI.Emulation.Snk.Emulators.Mame2010.Constants;

internal static class RuntimeOptionConstants
{
    internal const string CurrentXyType = "mame_current_xy_type";
    internal const string CurrentVideoapproach1Enable = "mame_current_videoapproach1_enable";
    internal const string CurrentSkipNagscreen = "mame_current_skip_nagscreen";
    internal const string CurrentSkipGameinfo = "mame_current_skip_gameinfo";
    internal const string CurrentSkipWarnings = "mame_current_skip_warnings";
    internal const string CurrentAspectRatio = "mame_current_aspect_ratio";
    internal const string CurrentTurboButton = "mame_current_turbo_button";
    internal const string CurrentTurboDelay = "mame_current_turbo_delay";
    internal const string CurrentFrameSkip = "mame_current_frame_skip";
    internal const string CurrentSampleRate = "mame_current_sample_rate";
    internal const string CurrentMultithreading = "mame_current_multithreading";
    internal const string CurrentAdjBrightness = "mame_current_adj_brightness";
    internal const string CurrentAdjContrast = "mame_current_adj_contrast";
    internal const string CurrentAdjGamma = "mame_current_adj_gamma";
    internal const string ExternalHiscore = "mame-external_hiscore";
    internal const string QsoundHle = "mame2010-qsound_hle";
    internal const string Disabled = "disabled";
    internal const string NoPointer = "none";
    internal const string SystemArchive = "neogeo.zip";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip", ".7z" });
}
