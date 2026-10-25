namespace GWGUI.Emulation.Snk.Emulators.Race.Constants;

internal static class RuntimeOptionConstants
{
    internal const string Language = "race_language";
    internal const string DarkFilterLevel = "race_dark_filter_level";
    internal const string Frameskip = "race_frameskip";
    internal const string FrameskipThreshold = "race_frameskip_threshold";
    internal const string RtcMode = "race_rtc_mode";
    internal const string AudioQuality = "race_audio_quality";
    internal const string AudioSamplerate = "race_audio_samplerate";
    internal const string ManualFrameskip = "manual";
    internal const string SaveExtension = ".ngf";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".ngp", ".ngc", ".ngpc", ".npc" });
}
