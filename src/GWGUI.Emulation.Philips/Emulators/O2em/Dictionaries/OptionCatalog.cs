using System.Globalization;
using GWGUI.Emulation.Philips.Emulators.Common.Functions;
using GWGUI.Emulation.Philips.Emulators.O2em.Constants;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Dictionaries;
internal static class O2emOptionCatalog
{
    private static readonly IReadOnlyList<CoreOptionValue> VolumeChoices = NumericOptionValueFunctions.IntegerChoices(OptionConstants.VolumePercentRanges, true);

    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.Bios,
            "Emulated Hardware (Restart)",
            "Specify which console hardware to emulate. Requires the corresponding bios file to be present in the frontend 'system' directory (o2rom.bin, c52.bin, g7400.bin, jopac.bin).",
            null,
            OptionConstants.O2romBin,
            OptionConstants.O2romBin,
            [
                new(OptionConstants.O2romBin, "Odyssey 2 (NTSC)", InvariantDisplayValue: "Odyssey 2 (NTSC)"),
                new(OptionConstants.C52Bin, "Videopac G7000 (European)", InvariantDisplayValue: "Videopac G7000 (European)"),
                new(OptionConstants.G7400Bin, "Videopac+ G7400 (European)", InvariantDisplayValue: "Videopac+ G7400 (European)"),
                new(OptionConstants.JopacBin, "Videopac+ G7400 (French)", InvariantDisplayValue: "Videopac+ G7400 (French)")
            ],
            IsVisible: false
        ),
        new(
            OptionConstants.Region,
            "Console Region (Restart)",
            "Specify which region the system is from. 'Auto' chooses the correct region based on emulated hardware. 'NTSC' is 60hz, 'PAL' is 50hz. Games may run abnormally if the wrong region is selected, and the setting my be overridden if the current content is incompatible.",
            null,
            OptionConstants.Auto,
            OptionConstants.Auto,
            [new(OptionConstants.Auto, "Auto", ResourceKey: SettingsChoiceConstants.Automatic), new(OptionConstants.NTSC, "NTSC", InvariantDisplayValue: "NTSC"), new(OptionConstants.PAL, "PAL", InvariantDisplayValue: "PAL")],
            ResourceKey: SettingsTextConstants.VideoStandard,
            ExplanationResourceKey: SettingsTextConstants.VideoStandardHelp
        ),
        new(
            OptionConstants.SwapGamepads,
            "Swap Gamepads",
            "Swap inputs from the two connected controllers of the emulated console. Required for games such as UFO and P.T. Barnum's Acrobats, which accept player 1 input from the second controller.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled), new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled)],
            ResourceKey: SettingsTextConstants.SwapGamepads
        ),
        new(
            OptionConstants.VkbdTransparency,
            "Virtual KBD Transparency",
            "Set transparency level of the virtual on-screen keyboard.",
            null,
            OptionConstants.DefaultKeyboardTransparencyPercent.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultKeyboardTransparencyPercent.ToString(CultureInfo.InvariantCulture),
            NumericOptionValueFunctions.IntegerChoices(OptionConstants.KeyboardTransparencyPercentRanges, true),
            IsVisible: false
        ),
        new(
            OptionConstants.CropOverscan,
            "Crop Overscan",
            "Remove the border around the edges of the screen, typically unused by games and hidden by the bezel of a standard-definition television.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled), new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled)],
            ResourceKey: SettingsTextConstants.CropOverscan,
            ExplanationResourceKey: SettingsTextConstants.CropOverscanHelp
        ),
        new(
            OptionConstants.MixFrames,
            "Interframe Blending",
            "Simulate CRT phosphor ghosting effects. 'Simple' performs a 50:50 mix of the current and previous frames. 'Ghosting' accumulates pixels from multiple successive frames. May be used to alleviate screen flicker.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [
                new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new(OptionConstants.Mix, "Simple", ResourceKey: SettingsChoiceConstants.Simple),
                new(OptionConstants.Ghost65, "Ghosting (65%)", InvariantDisplayValue: "65 %"),
                new(OptionConstants.Ghost75, "Ghosting (75%)", InvariantDisplayValue: "75 %"),
                new(OptionConstants.Ghost85, "Ghosting (85%)", InvariantDisplayValue: "85 %"),
                new(OptionConstants.Ghost95, "Ghosting (95%)", InvariantDisplayValue: "95 %")
            ],
            ResourceKey: SettingsTextConstants.MixFrames,
            ExplanationResourceKey: SettingsTextConstants.MixFramesHelp
        ),
        new(
            OptionConstants.AudioVolume,
            "Audio Volume",
            "Set output audio volume level.",
            null,
            OptionConstants.DefaultAudioVolumePercent.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultAudioVolumePercent.ToString(CultureInfo.InvariantCulture),
            VolumeChoices,
            ResourceKey: SettingsTextConstants.AudioVolume,
            ExplanationResourceKey: SettingsTextConstants.AudioVolumeHelp
        ),
        new(
            OptionConstants.VoiceVolume,
            "Voice Volume",
            "Set output volume level of 'The Voice' speech samples. The voice sampleset WAV files must be placed in the frontend 'system/voice' directory.",
            null,
            OptionConstants.DefaultVoiceVolumePercent.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultVoiceVolumePercent.ToString(CultureInfo.InvariantCulture),
            VolumeChoices,
            ResourceKey: SettingsTextConstants.VoiceVolume,
            ExplanationResourceKey: SettingsTextConstants.VoiceVolumeHelp
        ),
        new(
            OptionConstants.LowPassFilter,
            "Audio Filter",
            "Apply a low pass audio filter to soften the 'harsh' sound effects produced by most Odyssey2/Videopac+ games.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled), new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled)],
            ResourceKey: SettingsTextConstants.LowPassFilter,
            ExplanationResourceKey: SettingsTextConstants.LowPassFilterHelp
        ),
        new(
            OptionConstants.LowPassRange,
            "Audio Filter Level",
            "Specify the cut-off frequency of the low pass audio filter. A higher value increases the perceived 'strength' of the filter, since a wider range of the high frequency spectrum is attenuated.",
            null,
            OptionConstants.DefaultLowPassRangePercent.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultLowPassRangePercent.ToString(CultureInfo.InvariantCulture),
            NumericOptionValueFunctions.IntegerChoices(OptionConstants.LowPassPercentRanges, true),
            ResourceKey: SettingsTextConstants.LowPassRange,
            ExplanationResourceKey: SettingsTextConstants.LowPassRangeHelp
        )
    ];
}
