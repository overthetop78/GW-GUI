using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;

namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Dictionaries;
internal static class BlueMSXOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.OptionMsxtype,
            "Machine Type (Restart)",
            "Manually select the machine type you would like the core to start in.",
            "system",
            "Auto",
            "Auto",
            [
                new("Auto", "Auto"),
                new("MSX", "MSX"),
                new("MSXturboR", "MSXturboR"),
                new("MSX2", "MSX2"),
                new("MSX2+", "MSX2+"),
                new("SEGA - SG-1000", "SEGA - SG-1000"),
                new("SEGA - SC-3000", "SEGA - SC-3000"),
                new("SEGA - SF-7000", "SEGA - SF-7000"),
                new("SVI - Spectravideo SVI-318", "SVI - Spectravideo SVI-318"),
                new("SVI - Spectravideo SVI-328", "SVI - Spectravideo SVI-328"),
                new("SVI - Spectravideo SVI-328 MK2", "SVI - Spectravideo SVI-328 MK2"),
                new("ColecoVision", "ColecoVision"),
                new("Coleco (Spectravideo SVI-603)", "Coleco (Spectravideo SVI-603)")
            ]
        ),
        new(OptionConstants.OptionAutoRewindCas, "Auto Rewind Cassette", "", "system", "ON", "ON", [new("ON", "ON"), new("OFF", "OFF")]),
        new(
            OptionConstants.OptionRtcSource,
            "Real-Time Clock Source (Restart)",
            "Initial time of the emulated real-time clock (MSX2 and later). 'Host Clock' matches your computer's clock. 'Fixed Epoch' always starts at 1985-01-01 00:00:00, making emulation bit-identical between runs for netplay, run-ahead and input replay. The clock advances from emulated time either way.",
            "system",
            "host clock",
            "host clock",
            [new("host clock", "Host Clock"), new("fixed epoch", "Fixed Epoch (deterministic)")]
        ),
        new(
            OptionConstants.OptionOverscan,
            "Crop Overscan",
            "Forces cropping of overscanned frames",
            "video",
            "disabled",
            "disabled",
            [new("disabled", "disabled", ResourceKey: "Emulation.Value.Disabled"), new("enabled", "enabled", ResourceKey: "Emulation.Value.Enabled"), new("MSX2", "MSX2", ResourceKey: SettingsChoiceConstants.ValueMSX2)],
            ResourceKey: SettingsTextConstants.CropScreenBorders,
            ExplanationResourceKey: SettingsTextConstants.CropScreenBordersHelp
        ),
        new(
            OptionConstants.OptionVdpSynctype,
            "VDP Sync Type (Restart)",
            "Match the game/machine region frequency to avoid emulated speed issues.",
            "video",
            "Auto",
            "Auto",
            [new("Auto", "Auto", ResourceKey: "Emulation.Value.Auto"), new("50Hz", "50Hz", ResourceKey: SettingsChoiceConstants.ValueFrequency50Hz), new("60Hz", "60Hz", ResourceKey: SettingsChoiceConstants.ValueFrequency60Hz)],
            ResourceKey: SettingsTextConstants.RefreshRate,
            ExplanationResourceKey: SettingsTextConstants.RefreshRateHelp
        ),
        new(
            OptionConstants.OptionNospritelimits,
            "No Sprite Limit",
            "Remove the 4 sprite per line limit which can reduce or remove sprite flicker in some games.",
            "video",
            "OFF",
            "OFF",
            [new("OFF", "OFF", ResourceKey: "Emulation.Value.Disabled"), new("ON", "ON", ResourceKey: "Emulation.Value.Enabled")],
            ResourceKey: SettingsTextConstants.RemoveSpriteLimit,
            ExplanationResourceKey: SettingsTextConstants.RemoveSpriteLimitHelp
        ),
        new(OptionConstants.OptionYm2413Enable, "Sound YM2413 Enable (Restart)", "Awaiting description.", "audio", "enabled", "enabled", [new("enabled", "enabled"), new("disabled", "disabled")]),
        new(
            OptionConstants.OptionAdditionalCart,
            "Additional MSX Sound Cartridge (Restart)",
            "Additional MSX sound cartridge to insert to the first free slot.",
            "audio",
            "disabled",
            "disabled",
            [new("disabled", "disabled"), new("scc", "scc"), new("sccexpanded", "sccexpanded"), new("sccmirrored", "sccmirrored"), new("sccplus", "sccplus"), new("snatcher", "snatcher"), new("sdsnatcher", "sdsnatcher")]
        ),
        new(OptionConstants.OptionSoundIoEnable, "I/O Sound Enable", "Enable the I/O sound (floppy disk access sound).", "audio", "disabled", "disabled", [new("enabled", "enabled"), new("disabled", "disabled")]),
        new(
            OptionConstants.OptionUseKeyboardForColeco,
            "Colecovision Keyboard Mapping",
            "Additionally, use the keyboard for the Colecovision 0-9, #, and * buttons. Player 1: 0-9, # is -, and * is =; Player 2: Keypad 0-9, # is /, and * is *. Gamepad mappings also work.",
            "input",
            "disabled",
            "disabled",
            [new("enabled", "enabled"), new("disabled", "disabled")]
        ),
        new(
            OptionConstants.OptionHardResetF12,
            "Use F12 to hard reset and restart emulation",
            "Use key F12 to hard reset and restart emulation like blueMSX standalone",
            "input",
            "disabled",
            "disabled",
            [new("enabled", "enabled"), new("disabled", "disabled")]
        ),
        new(
            OptionConstants.OptionCartmapper,
            "Cart Mapper Type (Restart)",
            "When a ROM game or application is in the database, the emulator uses the databases to apply the correct mapper. If the sha1 value of a dump is not yet in the databases, it uses an automatic mapper detection system, but it can fail in some cases. In this situation, you can manually select the correct mapper.",
            "advanced",
            "Auto",
            "Auto",
            [
                new("Auto", "Auto"),
                new("Normal", "Normal"),
                new("mirrored", "mirrored"),
                new("basic", "basic"),
                new("0x4000", "0x4000"),
                new("0xC000", "0xC000"),
                new("ascii8", "ascii8"),
                new("ascii8sram", "ascii8sram"),
                new("ascii16", "ascii16"),
                new("ascii16sram", "ascii16sram"),
                new("ascii16nf", "ascii16nf"),
                new("konami4", "konami4"),
                new("konami4nf", "konami4nf"),
                new("konami5", "konami5"),
                new("konamisynth", "konamisynth"),
                new("korean80", "korean80"),
                new("korean90", "korean90"),
                new("korean126", "korean126"),
                new("MegaFlashRomScc", "MegaFlashRomScc"),
                new("MegaFlashRomSccPlus", "MegaFlashRomSccPlus"),
                new("SegaBasic", "SegaBasic"),
                new("SG1000", "SG1000"),
                new("SG1000Castle", "SG1000Castle"),
                new("SG1000RamA", "SG1000RamA"),
                new("SG1000RamB", "SG1000RamB"),
                new("SC3000", "SC3000"),
                new("SC3000MultiCart", "SC3000MultiCart"),
                new("SC3000MegaCart", "SC3000MegaCart")
            ]
        ),
        new(
            OptionConstants.OptionPatchColecoRom,
            "Patch Colecovision ROM Fast Boot (Close Content)",
            "Patch the NTSC Colecovision ROM for fast booting (3.3s)",
            "advanced",
            "disabled",
            "disabled",
            [new("enabled", "enabled", ResourceKey: "Emulation.Value.Enabled"), new("disabled", "disabled", ResourceKey: "Emulation.Value.Disabled")],
            ResourceKey: SettingsTextConstants.FastBoot,
            ExplanationResourceKey: SettingsTextConstants.FastBootHelp
        ),

    ];
}