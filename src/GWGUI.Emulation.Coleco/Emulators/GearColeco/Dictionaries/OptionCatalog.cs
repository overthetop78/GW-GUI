using GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants;

namespace GWGUI.Emulation.Coleco.Emulators.GearColeco.Dictionaries;
internal static class GearColecoOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.OptionCartridgeHardware,
            "Cartridge Hardware (restart)",
            "Select hardware for cartridge ROMs only. Disks, data packs, playlists and no-content startup always use ADAM.",
            "system",
            "ColecoVision",
            "ColecoVision",
            [new("ColecoVision", "ColecoVision"), new("ADAM", "ADAM")]
        ),
        new(
            OptionConstants.OptionAdamDiskDrive,
            "ADAM Disk Control Drive",
            "Choose which drive the frontend Disk Control menu operates. Other drives remain mounted. Loaded media selects the primary content drive.",
            "system",
            "Loaded media",
            "Loaded media",
            [new("Loaded media", "Loaded media"), new("Disk 1", "Disk 1"), new("Disk 2", "Disk 2"), new("Data Pack 1", "Data Pack 1"), new("Data Pack 2", "Data Pack 2")]
        ),
        new(
            OptionConstants.OptionAdamComputerReset,
            "ADAM Computer Reset",
            "Select Reset to boot the computer from mounted media, or SmartWriter when none is bootable. To request it again, select Idle then Reset. Normal frontend Reset retains the current boot mode.",
            "system",
            "Idle",
            "Idle",
            [new("Idle", "Idle"), new("Reset", "Reset")]
        ),
        new(
            OptionConstants.OptionAdamWritableMedia,
            "ADAM Writable Media (restart)",
            "Disabled mounts ADAM media write protected. Save-directory working copy stores writes in complete images under the frontend save directory and never overwrites source content.",
            "system",
            "Disabled",
            "Disabled",
            [new("Disabled", "Disabled"), new("Save-directory working copy", "Save-directory working copy")]
        ),
        new(
            OptionConstants.OptionTiming,
            "Refresh Rate (restart)",
            "Select which refresh rate will be used in emulation. 'Auto' automatically selects the best refresh rate based on the loaded content. 'NTSC (60 Hz)' forces 60 Hz. 'PAL (50 Hz)' forces 50 Hz.",
            "system",
            "Auto",
            "Auto",
            [
                new("Auto", "Auto", ResourceKey: "Emulation.Value.Auto"),
                new("NTSC (60 Hz)", "NTSC (60 Hz)", VideoStandard: EmulationVideoStandard.Ntsc, FrequencyHz: 60),
                new("PAL (50 Hz)", "PAL (50 Hz)", VideoStandard: EmulationVideoStandard.Pal, FrequencyHz: 50)
            ],
            ResourceKey: SettingsTextConstants.RefreshRate,
            ExplanationResourceKey: SettingsTextConstants.RefreshRateHelp
        ),
        new(
            OptionConstants.OptionMapper,
            "Mapper (restart)",
            "Select which mapper (memory bank controller) is emulated. 'Auto' automatically detects the appropriate mapper based on the loaded content. Only change this if a game does not work correctly with the default setting.",
            "system",
            "Auto",
            "Auto",
            [
                new("Auto", "Auto", ResourceKey: "Emulation.Value.Auto"),
                new("Standard", "Standard", ResourceKey: SettingsChoiceConstants.ValueStandard),
                new("MegaCart", "MegaCart", ResourceKey: SettingsChoiceConstants.ValueMegaCart),
                new("Activision", "Activision", ResourceKey: SettingsChoiceConstants.ValueActivision),
                new("OCM", "OCM", ResourceKey: SettingsChoiceConstants.ValueOCM)
            ],
            ResourceKey: SettingsTextConstants.CartridgeType,
            ExplanationResourceKey: SettingsTextConstants.CartridgeTypeHelp
        ),
        new(
            OptionConstants.OptionVideoChip,
            "Video Chip (restart)",
            "Select the installed video chip. 'Auto' uses TMS9918A unless the loaded game database entry requires F18A. Changes take effect when content is restarted.",
            "system",
            "Auto",
            "Auto",
            [new("Auto", "Auto", ResourceKey: "Emulation.Value.Auto"), new("TMS9918A", "TMS9918A", ResourceKey: SettingsChoiceConstants.ValueTMS9918A), new("F18A", "F18A", ResourceKey: SettingsChoiceConstants.ValueF18A)],
            ResourceKey: SettingsTextConstants.VideoChip,
            ExplanationResourceKey: SettingsTextConstants.VideoChipHelp
        ),
        new(
            OptionConstants.OptionAspectRatio,
            "Aspect Ratio",
            "Select which aspect ratio will be presented by the core. '1:1 PAR' selects an aspect ratio that produces square pixels.",
            "video",
            "1:1 PAR",
            "1:1 PAR",
            [
                new("1:1 PAR", "1:1 PAR", ResourceKey: SettingsChoiceConstants.ValueAspect1By1PAR),
                new("4:3 DAR", "4:3 DAR", ResourceKey: SettingsChoiceConstants.ValueAspect4By3DAR),
                new("16:9 DAR", "16:9 DAR", ResourceKey: SettingsChoiceConstants.ValueAspect16By9DAR),
                new("16:10 DAR", "16:10 DAR", ResourceKey: SettingsChoiceConstants.ValueAspect16By10DAR)
            ],
            ResourceKey: SettingsTextConstants.AspectRatio,
            ExplanationResourceKey: SettingsTextConstants.AspectRatioHelp
        ),
        new(
            OptionConstants.OptionOverscan,
            "Overscan",
            "Select which overscan (borders) will be used in TMS9918A emulation. 'Disabled' shows no overscan. 'Top+Bottom' enables vertical overscan only. 'Full (284 width)' and 'Full (320 width)' enable full overscan with different horizontal widths. F18A output uses its active logical raster without TMS overscan.",
            "video",
            "Disabled",
            "Disabled",
            [
                new("Disabled", "Disabled", ResourceKey: "Emulation.Value.Disabled"),
                new("Top+Bottom", "Top+Bottom", ResourceKey: SettingsChoiceConstants.ValueTopAndBottomBorders),
                new("Full (284 width)", "Full (284 width)", ResourceKey: SettingsChoiceConstants.ValueFullBorders284),
                new("Full (320 width)", "Full (320 width)", ResourceKey: SettingsChoiceConstants.ValueFullBorders320)
            ],
            ResourceKey: SettingsTextConstants.ScreenBorders,
            ExplanationResourceKey: SettingsTextConstants.ScreenBordersHelp
        ),
        new(
            OptionConstants.OptionNoSpriteLimit,
            "No Sprite Limit",
            "Remove the per-line sprite limit. This reduces flickering but may cause glitches in certain games. It's best to keep this option disabled.",
            "video",
            "Disabled",
            "Disabled",
            [new("Disabled", "Disabled", ResourceKey: "Emulation.Value.Disabled"), new("Enabled", "Enabled", ResourceKey: "Emulation.Value.Enabled")],
            ResourceKey: SettingsTextConstants.RemoveSpriteLimit,
            ExplanationResourceKey: SettingsTextConstants.RemoveSpriteLimitHelp
        ),
        new(
            OptionConstants.OptionUpDownAllowed,
            "Allow Up+Down / Left+Right",
            "Allow pressing, quickly alternating, or holding both left and right (or up and down) directions at the same time. This may cause movement based glitches in certain games.",
            "input",
            "Disabled",
            "Disabled",
            [new("Disabled", "Disabled", ResourceKey: "Emulation.Value.Disabled"), new("Enabled", "Enabled", ResourceKey: "Emulation.Value.Enabled")],
            ResourceKey: SettingsTextConstants.AllowOppositeDirections,
            ExplanationResourceKey: SettingsTextConstants.AllowOppositeDirectionsHelp
        ),
        new(
            OptionConstants.OptionSpinners,
            "Spinner Support",
            "Select which spinner controller will be emulated. Spinners are controlled by mouse movement. Mouse buttons are mapped to Left (Yellow) and Right (Red) buttons.",
            "input",
            "Disabled",
            "Disabled",
            [new("Disabled", "Disabled"), new("Super Action Controller", "Super Action Controller"), new("Wheel Controller", "Wheel Controller"), new("Roller Controller", "Roller Controller")]
        ),
        new(
            OptionConstants.OptionSpinnerSensitivity,
            "Spinner Sensitivity",
            "Set the sensitivity of the spinner controller. 1 is the lowest sensitivity and 10 is the highest.",
            "input",
            "1",
            "1",
            [
                new("1", "1", ResourceKey: SettingsChoiceConstants.ValueNumber1),
                new("2", "2", ResourceKey: SettingsChoiceConstants.ValueNumber2),
                new("3", "3", ResourceKey: SettingsChoiceConstants.ValueNumber3),
                new("4", "4", ResourceKey: SettingsChoiceConstants.ValueNumber4),
                new("5", "5", ResourceKey: SettingsChoiceConstants.ValueNumber5),
                new("6", "6", ResourceKey: SettingsChoiceConstants.ValueNumber6),
                new("7", "7", ResourceKey: SettingsChoiceConstants.ValueNumber7),
                new("8", "8", ResourceKey: SettingsChoiceConstants.ValueNumber8),
                new("9", "9", ResourceKey: SettingsChoiceConstants.ValueNumber9),
                new("10", "10", ResourceKey: SettingsChoiceConstants.ValueNumber10)
            ],
            ResourceKey: SettingsTextConstants.RotarySensitivity,
            ExplanationResourceKey: SettingsTextConstants.RotarySensitivityHelp
        ),

    ];
}