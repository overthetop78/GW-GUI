using GWGUI.Emulation.Snk.Emulators.Mame2000.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2000.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
        RuntimeOptionConstants.Frameskip,
        "Frameskip",
        "Skip rendering frames to keep the audio output in sync when the host cannot run the game at full speed.  'auto' uses the frontend's audio-buffer hint; 'threshold' skips when the audio buffer drops below the percentage configured below.",
        EmulationMachineTab.Video.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("auto", "auto"), new("threshold", "threshold", ResourceKey: SettingsChoiceConstants.ValueThreshold)],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingFrameskip,
        ExplanationResourceKey: SettingsTextConstants.HelpFrameskip
    ), new(
        RuntimeOptionConstants.FrameskipThreshold,
        "Frameskip Threshold (%)",
        "When Frameskip is set to 'threshold', skip a frame whenever the audio output buffer falls below this percentage of full.  Lower values skip more aggressively.",
        EmulationMachineTab.Video.ToString(),
        "30",
        "30",
        [
        new("30", "30", ResourceKey: SettingsChoiceConstants.Value30),
        new("40", "40", ResourceKey: SettingsChoiceConstants.Value40),
        new("50", "50", ResourceKey: SettingsChoiceConstants.Value50),
        new("60", "60", ResourceKey: SettingsChoiceConstants.Value60)
    ],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingFrameskipThreshold,
        ExplanationResourceKey: SettingsTextConstants.HelpFrameskipThreshold
    ), new(
        RuntimeOptionConstants.FrameskipInterval,
        "Frameskip Interval",
        "Maximum number of consecutive frames that may be skipped.  Lower values keep visual updates smoother at the cost of more audio drops when the host is overloaded.",
        EmulationMachineTab.Video.ToString(),
        "1",
        "1",
        [
        new("1", "1", ResourceKey: SettingsChoiceConstants.Value1),
        new("2", "2", ResourceKey: SettingsChoiceConstants.Value2),
        new("3", "3", ResourceKey: SettingsChoiceConstants.Value3),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("5", "5", ResourceKey: SettingsChoiceConstants.Value5),
        new("6", "6", ResourceKey: SettingsChoiceConstants.Value6),
        new("7", "7", ResourceKey: SettingsChoiceConstants.Value7),
        new("8", "8", ResourceKey: SettingsChoiceConstants.Value8),
        new("9", "9", ResourceKey: SettingsChoiceConstants.Value9)
    ],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingFrameskipInterval,
        ExplanationResourceKey: SettingsTextConstants.HelpFrameskipInterval
    ), new(
        RuntimeOptionConstants.SkipDisclaimer,
        "Skip Disclaimer",
        "Hide MAME's legal disclaimer screen at game startup.",
        EmulationMachineTab.General.ToString(),
        "enabled",
        "enabled",
        [new("enabled", "enabled"), new("disabled", "disabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingSkipDisclaimer,
        ExplanationResourceKey: SettingsTextConstants.HelpSkipDisclaimer
    ), new(
        RuntimeOptionConstants.ShowGameInfo,
        "Show Game Information",
        "Show MAME's driver-status and game-information screens at startup before the game begins running.",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingShowGameInformation,
        ExplanationResourceKey: SettingsTextConstants.HelpShowGameInformation
    ), new(
        RuntimeOptionConstants.SampleRate,
        "Sample rate hint",
        "Output audio sample rate, in Hz.  22050 is the legacy MAME 0.78 default; higher rates increase audio fidelity at the cost of additional CPU.  \"Auto\" asks the frontend for its target rate and rounds to the nearest standard rate, falling back to 48000 if the frontend does not report one.  \"Manual\" currently behaves like \"Auto\".",
        EmulationMachineTab.Audio.ToString(),
        "22050",
        "22050",
        [
        new("auto", "Auto"),
        new("manual", "Manual", ResourceKey: SettingsChoiceConstants.ValueManual),
        new("8000", "8 kHz", ResourceKey: SettingsChoiceConstants.Value8kHz),
        new("11025", "11.025 kHz", ResourceKey: SettingsChoiceConstants.Value11Point025kHz),
        new("22050", "22.05 kHz", ResourceKey: SettingsChoiceConstants.Value22Point05kHz),
        new("32000", "32 kHz", ResourceKey: SettingsChoiceConstants.Value32kHz),
        new("44100", "44.1 kHz", ResourceKey: SettingsChoiceConstants.Value44Point1kHz),
        new("48000", "48 kHz", ResourceKey: SettingsChoiceConstants.Value48kHz),
        new("96000", "96 kHz", ResourceKey: SettingsChoiceConstants.Value96kHz)
    ],
        IsVisible: true,
        RequiresRestart: true,
        ResourceKey: SettingsTextConstants.SettingSampleRateHint,
        ExplanationResourceKey: SettingsTextConstants.HelpSampleRateHint
    ), new(
        RuntimeOptionConstants.Stereo,
        "Stereo",
        "Output stereo audio.  Disable for slightly lower CPU on games that don't use stereo mixing.",
        EmulationMachineTab.Audio.ToString(),
        "enabled",
        "enabled",
        [new("enabled", "enabled"), new("disabled", "disabled")],
        IsVisible: true,
        RequiresRestart: true,
        ResourceKey: SettingsTextConstants.SettingStereo,
        ExplanationResourceKey: SettingsTextConstants.HelpStereo
    ), new(
        RuntimeOptionConstants.QsoundOutputFilter,
        "QSound Output Filter",
        "Run the QSound voice mix through a 95-tap windowed-sinc low-pass FIR (8 kHz cutoff) and per-channel output delay line before sending it to the host.  Smooths the upper-band aliasing the 8-bit ROM samples produce on transients and applies the algorithm's slight L/R offset for a wider stereo image.  Costs roughly 15-20% extra CPU during active QSound audio.  This option is hidden in the menu for games that don't use the QSound sound chip.",
        EmulationMachineTab.Audio.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: false,
        RequiresRestart: false
    ), });
}
