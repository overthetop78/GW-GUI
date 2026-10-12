using GWGUI.Emulation.Snk.Emulators.Race.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Race.Dictionaries;
internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
            RuntimeOptionConstants.Language,
            "Language",
            "Specify which language to display when running content with dual language support.",
            EmulationMachineTab.General.ToString(),
            "english",
            "english",
            [new("japanese", "Japanese", ResourceKey: SettingsChoiceConstants.ValueJapanese), new("english", "English", ResourceKey: SettingsChoiceConstants.ValueEnglish)],
            RequiresRestart: true,
            ResourceKey: SettingsTextConstants.SettingLanguage,
            ExplanationResourceKey: SettingsTextConstants.HelpLanguage
        ), new(
            RuntimeOptionConstants.DarkFilterLevel,
            "Dark Filter Level (%)",
            "Enable luminosity-based brightness reduction. May be used to avoid glare/eye strain when playing games with white backgrounds.",
            EmulationMachineTab.Video.ToString(),
            "0",
            "0",
            [
                new("0", "0", ResourceKey: SettingsChoiceConstants.Value0),
                new("5", "5", ResourceKey: SettingsChoiceConstants.Value5),
                new("10", "10", ResourceKey: SettingsChoiceConstants.Value10),
                new("15", "15", ResourceKey: SettingsChoiceConstants.Value15),
                new("20", "20", ResourceKey: SettingsChoiceConstants.Value20),
                new("25", "25", ResourceKey: SettingsChoiceConstants.Value25),
                new("30", "30", ResourceKey: SettingsChoiceConstants.Value30),
                new("35", "35", ResourceKey: SettingsChoiceConstants.Value35),
                new("40", "40", ResourceKey: SettingsChoiceConstants.Value40),
                new("45", "45", ResourceKey: SettingsChoiceConstants.Value45),
                new("50", "50", ResourceKey: SettingsChoiceConstants.Value50)
            ],
            RequiresRestart: false,
            ResourceKey: SettingsTextConstants.SettingDarkFilterLevel,
            ExplanationResourceKey: SettingsTextConstants.HelpDarkFilterLevel
        ), new(
            RuntimeOptionConstants.Frameskip,
            "Frameskip",
            "Skip frames to avoid audio buffer under-run (crackling). Improves performance at the expense of visual smoothness. 'Auto' skips frames when advised by the frontend. 'Manual' utilises the 'Frameskip Threshold (%)' setting.",
            EmulationMachineTab.Video.ToString(),
            "disabled",
            "disabled",
            [new("disabled", "disabled"), new("auto", "Auto"), new("manual", "Manual", ResourceKey: SettingsChoiceConstants.ValueManual)],
            RequiresRestart: false,
            ResourceKey: SettingsTextConstants.SettingFrameskip,
            ExplanationResourceKey: SettingsTextConstants.HelpFrameskip
        ), new(
            RuntimeOptionConstants.FrameskipThreshold,
            "Frameskip Threshold (%)",
            "When 'Frameskip' is set to 'Manual', specifies the audio buffer occupancy threshold (percentage) below which frames will be skipped. Higher values reduce the risk of crackling by causing frames to be dropped more frequently.",
            EmulationMachineTab.Video.ToString(),
            "33",
            "33",
            [
                new("15", "15", ResourceKey: SettingsChoiceConstants.Value15),
                new("18", "18", ResourceKey: SettingsChoiceConstants.Value18),
                new("21", "21", ResourceKey: SettingsChoiceConstants.Value21),
                new("24", "24", ResourceKey: SettingsChoiceConstants.Value24),
                new("27", "27", ResourceKey: SettingsChoiceConstants.Value27),
                new("30", "30", ResourceKey: SettingsChoiceConstants.Value30),
                new("33", "33", ResourceKey: SettingsChoiceConstants.Value33),
                new("36", "36", ResourceKey: SettingsChoiceConstants.Value36),
                new("39", "39", ResourceKey: SettingsChoiceConstants.Value39),
                new("42", "42", ResourceKey: SettingsChoiceConstants.Value42),
                new("45", "45", ResourceKey: SettingsChoiceConstants.Value45),
                new("48", "48", ResourceKey: SettingsChoiceConstants.Value48),
                new("51", "51", ResourceKey: SettingsChoiceConstants.Value51),
                new("54", "54", ResourceKey: SettingsChoiceConstants.Value54),
                new("57", "57", ResourceKey: SettingsChoiceConstants.Value57),
                new("60", "60", ResourceKey: SettingsChoiceConstants.Value60)
            ],
            RequiresRestart: false,
            ResourceKey: SettingsTextConstants.SettingFrameskipThreshold,
            ExplanationResourceKey: SettingsTextConstants.HelpFrameskipThreshold
        ), new(
            RuntimeOptionConstants.RtcMode,
            "Real-Time Clock",
            "Selects how the emulated real-time clock advances. 'Deterministic' counts emulated frames from a fixed base, so RTC reads are reproducible and safe for netplay, rewind and run-ahead. 'Real Time' reads the host clock directly (a live clock, but non-deterministic).",
            EmulationMachineTab.General.ToString(),
            "deterministic",
            "deterministic",
            [new("deterministic", "Deterministic", ResourceKey: SettingsChoiceConstants.ValueDeterministic), new("realtime", "Real Time", ResourceKey: SettingsChoiceConstants.ValueRealTime)],
            RequiresRestart: false,
            ResourceKey: SettingsTextConstants.SettingRealTimeClock,
            ExplanationResourceKey: SettingsTextConstants.HelpRealTimeClock
        ), new(
            RuntimeOptionConstants.AudioQuality,
            "Audio Quality",
            "Selects the sound synthesis method. 'Fast' uses a lightweight per-sample mixer (default). 'Accurate' uses band-limited Blip synthesis that matches the Mednafen reference and removes high-frequency aliasing. The CPU cost of the accurate path is small (the audio is a minor fraction of total emulation time), but it is left off by default for maximum performance headroom on low-end hardware.",
            EmulationMachineTab.Audio.ToString(),
            "fast",
            "fast",
            [new("fast", "Fast", ResourceKey: SettingsChoiceConstants.ValueFast), new("accurate", "Accurate (Band-limited)", ResourceKey: SettingsChoiceConstants.ValueAccurateBandLimited)],
            RequiresRestart: true,
            ResourceKey: SettingsTextConstants.SettingAudioQuality,
            ExplanationResourceKey: SettingsTextConstants.HelpAudioQuality
        ), new(
            RuntimeOptionConstants.AudioSamplerate,
            "Audio Sample Rate",
            "Selects the output sample rate of the real-time sound synthesis. The NGPC has no sample-based sound hardware, so there is no single 'native' rate; higher rates reduce latency, eliminate aliasing, avoid low-pass smearing and give better time-domain resolution, at a small increase in CPU cost. 'Auto' queries the frontend's target rate and picks the closest supported value.",
            EmulationMachineTab.Audio.ToString(),
            "auto",
            "auto",
            [
                new("auto", "Auto"),
                new("32000", "32 kHz", ResourceKey: SettingsChoiceConstants.Value32kHz),
                new("44100", "44.1 kHz", ResourceKey: SettingsChoiceConstants.Value44Point1kHz),
                new("48000", "48 kHz", ResourceKey: SettingsChoiceConstants.Value48kHz)
            ],
            RequiresRestart: true,
            ResourceKey: SettingsTextConstants.SettingAudioSampleRate,
            ExplanationResourceKey: SettingsTextConstants.HelpAudioSampleRate
        ), });
}