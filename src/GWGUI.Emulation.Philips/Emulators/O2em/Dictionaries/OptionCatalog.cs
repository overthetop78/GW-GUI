using GWGUI.Emulation.Philips.Emulators.O2em.Constants;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Dictionaries;
internal static class O2emOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.Bios,
            "Emulated Hardware (Restart)",
            "Specify which console hardware to emulate. Requires the corresponding bios file to be present in the frontend 'system' directory (o2rom.bin, c52.bin, g7400.bin, jopac.bin).",
            null,
            OptionConstants.O2romBin,
            OptionConstants.O2romBin,
            [
                new(OptionConstants.O2romBin, "Odyssey 2 (NTSC)"),
                new(OptionConstants.C52Bin, "Videopac G7000 (European)"),
                new(OptionConstants.G7400Bin, "Videopac+ G7400 (European)"),
                new(OptionConstants.JopacBin, "Videopac+ G7400 (French)")
            ]
        ),
        new(
            OptionConstants.Region,
            "Console Region (Restart)",
            "Specify which region the system is from. 'Auto' chooses the correct region based on emulated hardware. 'NTSC' is 60hz, 'PAL' is 50hz. Games may run abnormally if the wrong region is selected, and the setting my be overridden if the current content is incompatible.",
            null,
            OptionConstants.Auto,
            OptionConstants.Auto,
            [new(OptionConstants.Auto, "Auto"), new(OptionConstants.NTSC, "NTSC"), new(OptionConstants.PAL, "PAL")]
        ),
        new(
            OptionConstants.SwapGamepads,
            "Swap Gamepads",
            "Swap inputs from the two connected controllers of the emulated console. Required for games such as UFO and P.T. Barnum's Acrobats, which accept player 1 input from the second controller.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "disabled"), new(OptionConstants.Enabled, "enabled")]
        ),
        new(
            OptionConstants.VkbdTransparency,
            "Virtual KBD Transparency",
            "Set transparency level of the virtual on-screen keyboard.",
            null,
            OptionConstants.Value0,
            OptionConstants.Value0,
            [new(OptionConstants.Value0, "0%"), new(OptionConstants.Value25, "25%"), new(OptionConstants.Value50, "50%"), new(OptionConstants.Value75, "75%")]
        ),
        new(
            OptionConstants.CropOverscan,
            "Crop Overscan",
            "Remove the border around the edges of the screen, typically unused by games and hidden by the bezel of a standard-definition television.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "disabled"), new(OptionConstants.Enabled, "enabled")]
        ),
        new(
            OptionConstants.MixFrames,
            "Interframe Blending",
            "Simulate CRT phosphor ghosting effects. 'Simple' performs a 50:50 mix of the current and previous frames. 'Ghosting' accumulates pixels from multiple successive frames. May be used to alleviate screen flicker.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [
                new(OptionConstants.Disabled, "disabled"),
                new(OptionConstants.Mix, "Simple"),
                new(OptionConstants.Ghost65, "Ghosting (65%)"),
                new(OptionConstants.Ghost75, "Ghosting (75%)"),
                new(OptionConstants.Ghost85, "Ghosting (85%)"),
                new(OptionConstants.Ghost95, "Ghosting (95%)")
            ]
        ),
        new(
            OptionConstants.AudioVolume,
            "Audio Volume",
            "Set output audio volume level.",
            null,
            OptionConstants.Value50,
            OptionConstants.Value50,
            [
                new(OptionConstants.Value0, "0%"),
                new(OptionConstants.Value5, "5%"),
                new(OptionConstants.Value10, "10%"),
                new(OptionConstants.Value15, "15%"),
                new(OptionConstants.Value20, "20%"),
                new(OptionConstants.Value25, "25%"),
                new(OptionConstants.Value30, "30%"),
                new(OptionConstants.Value35, "35%"),
                new(OptionConstants.Value40, "40%"),
                new(OptionConstants.Value45, "45%"),
                new(OptionConstants.Value50, "50%"),
                new(OptionConstants.Value55, "55%"),
                new(OptionConstants.Value60, "60%"),
                new(OptionConstants.Value65, "65%"),
                new(OptionConstants.Value70, "70%"),
                new(OptionConstants.Value75, "75%"),
                new(OptionConstants.Value80, "80%"),
                new(OptionConstants.Value85, "85%"),
                new(OptionConstants.Value90, "90%"),
                new(OptionConstants.Value95, "95%"),
                new(OptionConstants.Value100, "100%")
            ]
        ),
        new(
            OptionConstants.VoiceVolume,
            "Voice Volume",
            "Set output volume level of 'The Voice' speech samples. The voice sampleset WAV files must be placed in the frontend 'system/voice' directory.",
            null,
            OptionConstants.Value70,
            OptionConstants.Value70,
            [
                new(OptionConstants.Value0, "0%"),
                new(OptionConstants.Value5, "5%"),
                new(OptionConstants.Value10, "10%"),
                new(OptionConstants.Value15, "15%"),
                new(OptionConstants.Value20, "20%"),
                new(OptionConstants.Value25, "25%"),
                new(OptionConstants.Value30, "30%"),
                new(OptionConstants.Value35, "35%"),
                new(OptionConstants.Value40, "40%"),
                new(OptionConstants.Value45, "45%"),
                new(OptionConstants.Value50, "50%"),
                new(OptionConstants.Value55, "55%"),
                new(OptionConstants.Value60, "60%"),
                new(OptionConstants.Value65, "65%"),
                new(OptionConstants.Value70, "70%"),
                new(OptionConstants.Value75, "75%"),
                new(OptionConstants.Value80, "80%"),
                new(OptionConstants.Value85, "85%"),
                new(OptionConstants.Value90, "90%"),
                new(OptionConstants.Value95, "95%"),
                new(OptionConstants.Value100, "100%")
            ]
        ),
        new(
            OptionConstants.LowPassFilter,
            "Audio Filter",
            "Apply a low pass audio filter to soften the 'harsh' sound effects produced by most Odyssey2/Videopac+ games.",
            null,
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "disabled"), new(OptionConstants.Enabled, "enabled")]
        ),
        new(
            OptionConstants.LowPassRange,
            "Audio Filter Level",
            "Specify the cut-off frequency of the low pass audio filter. A higher value increases the perceived 'strength' of the filter, since a wider range of the high frequency spectrum is attenuated.",
            null,
            OptionConstants.Value60,
            OptionConstants.Value60,
            [
                new(OptionConstants.Value5, "5%"),
                new(OptionConstants.Value10, "10%"),
                new(OptionConstants.Value15, "15%"),
                new(OptionConstants.Value20, "20%"),
                new(OptionConstants.Value25, "25%"),
                new(OptionConstants.Value30, "30%"),
                new(OptionConstants.Value35, "35%"),
                new(OptionConstants.Value40, "40%"),
                new(OptionConstants.Value45, "45%"),
                new(OptionConstants.Value50, "50%"),
                new(OptionConstants.Value55, "55%"),
                new(OptionConstants.Value60, "60%"),
                new(OptionConstants.Value65, "65%"),
                new(OptionConstants.Value70, "70%"),
                new(OptionConstants.Value75, "75%"),
                new(OptionConstants.Value80, "80%"),
                new(OptionConstants.Value85, "85%"),
                new(OptionConstants.Value90, "90%"),
                new(OptionConstants.Value95, "95%")
            ]
        )
    ];
}