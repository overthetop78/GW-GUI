using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Dictionaries;
internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(OptionConstants.Menufontsize, "Menu Font Size", null, null, "normal", "normal", [new("normal", "normal"), new("large", "large")], IsVisible: false), new(
            OptionConstants.Cpuspeed,
            "CPU Speed",
            null,
            EmulationMachineTab.Cpu.ToString(),
            "10Mhz",
            "10Mhz",
            [
                new("10Mhz", "10Mhz", InvariantDisplayValue: "10 MHz"),
                new("16Mhz", "16Mhz", InvariantDisplayValue: "16 MHz"),
                new("25Mhz", "25Mhz", InvariantDisplayValue: "25 MHz"),
                new("33Mhz (OC)", "33Mhz (OC)", InvariantDisplayValue: "33 MHz"),
                new("66Mhz (OC)", "66Mhz (OC)", InvariantDisplayValue: "66 MHz"),
                new("100Mhz (OC)", "100Mhz (OC)", InvariantDisplayValue: "100 MHz")
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.CpuSpeed,
            ExplanationResourceKey: SettingsTextConstants.CpuSpeedHelp
        ), new(
            OptionConstants.Ramsize,
            "RAM Size",
            null,
            EmulationMachineTab.Ram.ToString(),
            "2MB",
            "2MB",
            [
                new("2MB", "2MB", InvariantDisplayValue: "2 MiB"),
                new("1MB", "1MB", InvariantDisplayValue: "1 MiB"),
                new("3MB", "3MB", InvariantDisplayValue: "3 MiB"),
                new("4MB", "4MB", InvariantDisplayValue: "4 MiB"),
                new("5MB", "5MB", InvariantDisplayValue: "5 MiB"),
                new("6MB", "6MB", InvariantDisplayValue: "6 MiB"),
                new("7MB", "7MB", InvariantDisplayValue: "7 MiB"),
                new("8MB", "8MB", InvariantDisplayValue: "8 MiB"),
                new("9MB", "9MB", InvariantDisplayValue: "9 MiB"),
                new("10MB", "10MB", InvariantDisplayValue: "10 MiB"),
                new("11MB", "11MB", InvariantDisplayValue: "11 MiB"),
                new("12MB", "12MB", InvariantDisplayValue: "12 MiB")
            ],
            IsVisible: true,
            RequiresRestart: true,
            ResourceKey: SettingsTextConstants.MemorySize,
            ExplanationResourceKey: SettingsTextConstants.MemorySizeHelp
        ), new(OptionConstants.Analog, "Use Analog", null, null, "disabled", "disabled", [new("disabled", "disabled"), new("enabled", "enabled")], IsVisible: false), new(
            OptionConstants.Joytype1,
            "P1 Joypad Type",
            null,
            null,
            "Default (2 Buttons)",
            "Default (2 Buttons)",
            [
                new("Default (2 Buttons)", "Default (2 Buttons)"),
                new("CPSF-MD (8 Buttons)", "CPSF-MD (8 Buttons)"),
                new("CPSF-SFC (8 Buttons)", "CPSF-SFC (8 Buttons)"),
                new("Cyberstick (Digital)", "Cyberstick (Digital)"),
                new("Cyberstick (Analog)", "Cyberstick (Analog)")
            ],
            IsVisible: false
        ), new(
            OptionConstants.Joytype2,
            "P2 Joypad Type",
            null,
            null,
            "Default (2 Buttons)",
            "Default (2 Buttons)",
            [new("Default (2 Buttons)", "Default (2 Buttons)"), new("CPSF-MD (8 Buttons)", "CPSF-MD (8 Buttons)"), new("CPSF-SFC (8 Buttons)", "CPSF-SFC (8 Buttons)")],
            IsVisible: false
        ), new(
            OptionConstants.Joy1Select,
            "P1 Joystick Select Mapping",
            null,
            null,
            "Default",
            "Default",
            [new("Default", "Default"), new("XF1", "XF1"), new("XF2", "XF2"), new("XF3", "XF3"), new("XF4", "XF4"), new("XF5", "XF5"), new("OPT1", "OPT1"), new("OPT2", "OPT2"), new("F1", "F1"), new("F2", "F2")],
            IsVisible: false
        ), new(OptionConstants.MidiOutput,
            "MIDI Output",
            null,
            null,
            "enabled",
            "enabled",
            [new("enabled", "enabled"),
            new("disabled", "disabled")],
            IsVisible: false,
            RequiresRestart: true), new(OptionConstants.MidiOutputType,
            "MIDI Output Type",
            null,
            null,
            "GM",
            "GM",
            [new("GM", "GM"),
            new("LA", "LA"),
            new("GS", "GS"),
            new("XG", "XG")],
            IsVisible: false), new(
            OptionConstants.AdpcmVol,
            "ADPCM Volume",
            null,
            EmulationMachineTab.Audio.ToString(),
            "15",
            "15",
            [
                new("15", "15", InvariantDisplayValue: "15"),
                new("0", "0", InvariantDisplayValue: "0"),
                new("1", "1", InvariantDisplayValue: "1"),
                new("2", "2", InvariantDisplayValue: "2"),
                new("3", "3", InvariantDisplayValue: "3"),
                new("4", "4", InvariantDisplayValue: "4"),
                new("5", "5", InvariantDisplayValue: "5"),
                new("6", "6", InvariantDisplayValue: "6"),
                new("7", "7", InvariantDisplayValue: "7"),
                new("8", "8", InvariantDisplayValue: "8"),
                new("9", "9", InvariantDisplayValue: "9"),
                new("10", "10", InvariantDisplayValue: "10"),
                new("11", "11", InvariantDisplayValue: "11"),
                new("12", "12", InvariantDisplayValue: "12"),
                new("13", "13", InvariantDisplayValue: "13"),
                new("14", "14", InvariantDisplayValue: "14")
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.AdpcmVolume,
            ExplanationResourceKey: SettingsTextConstants.AdpcmVolumeHelp
        ), new(
            OptionConstants.OpmVol,
            "OPM Volume",
            null,
            EmulationMachineTab.Audio.ToString(),
            "12",
            "12",
            [
                new("12", "12", InvariantDisplayValue: "12"),
                new("0", "0", InvariantDisplayValue: "0"),
                new("1", "1", InvariantDisplayValue: "1"),
                new("2", "2", InvariantDisplayValue: "2"),
                new("3", "3", InvariantDisplayValue: "3"),
                new("4", "4", InvariantDisplayValue: "4"),
                new("5", "5", InvariantDisplayValue: "5"),
                new("6", "6", InvariantDisplayValue: "6"),
                new("7", "7", InvariantDisplayValue: "7"),
                new("8", "8", InvariantDisplayValue: "8"),
                new("9", "9", InvariantDisplayValue: "9"),
                new("10", "10", InvariantDisplayValue: "10"),
                new("11", "11", InvariantDisplayValue: "11"),
                new("13", "13", InvariantDisplayValue: "13"),
                new("14", "14", InvariantDisplayValue: "14"),
                new("15", "15", InvariantDisplayValue: "15")
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.FmVolume,
            ExplanationResourceKey: SettingsTextConstants.FmVolumeHelp
        ), new(OptionConstants.DiskDrive,
            "Swap Disks on Drive",
            null,
            null,
            "FDD1",
            "FDD1",
            [new("FDD1", "FDD1"),
            new("FDD0", "FDD0")],
            IsVisible: false), new(OptionConstants.SaveFddPath,
            "Save FDD Paths",
            null,
            null,
            "enabled",
            "enabled",
            [new("enabled", "enabled"),
            new("disabled", "disabled")],
            IsVisible: false), new(OptionConstants.SaveHddPath,
            "Save HDD Paths",
            null,
            null,
            "enabled",
            "enabled",
            [new("enabled", "enabled"),
            new("disabled", "disabled")],
            IsVisible: false), new(OptionConstants.RumbleOnDiskRead,
            "Rumble on FDD Reads",
            null,
            null,
            "disabled",
            "disabled",
            [new("disabled", "disabled"),
            new("enabled", "enabled")],
            IsVisible: false), new(OptionConstants.JoyMouse,
            "Joy/Mouse",
            null,
            null,
            "Mouse",
            "Mouse",
            [new("Mouse", "Mouse"),
            new("Joystick", "Joystick")],
            IsVisible: false), new(
            OptionConstants.VbtnSwap,
            "VBtn Swap",
            null,
            EmulationMachineTab.Controllers.ToString(),
            "TRIG1 TRIG2",
            "TRIG1 TRIG2",
            [
                new("TRIG1 TRIG2", "TRIG1 TRIG2", InvariantDisplayValue: "TRIG1 + TRIG2"),
                new("TRIG2 TRIG1", "TRIG2 TRIG1", InvariantDisplayValue: "TRIG2 + TRIG1")
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.ButtonOrder,
            ExplanationResourceKey: SettingsTextConstants.ButtonOrderHelp
        ), new(
            OptionConstants.NoWaitMode,
            "No Wait Mode",
            null,
            EmulationMachineTab.Cpu.ToString(),
            "disabled",
            "disabled",
            [
                new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.UnlimitedSpeed,
            ExplanationResourceKey: SettingsTextConstants.UnlimitedSpeedHelp
        ), new(
            OptionConstants.Frameskip,
            "Frames Skip",
            null,
            EmulationMachineTab.Video.ToString(),
            "Full Frame",
            "Full Frame",
            [
                new("Full Frame", "Full Frame", ResourceKey: SettingsChoiceConstants.Disabled),
                new("1/2 Frame", "1/2 Frame", InvariantDisplayValue: "1/2"),
                new("1/3 Frame", "1/3 Frame", InvariantDisplayValue: "2/3"),
                new("1/4 Frame", "1/4 Frame", InvariantDisplayValue: "3/4"),
                new("1/5 Frame", "1/5 Frame", InvariantDisplayValue: "4/5"),
                new("1/6 Frame", "1/6 Frame", InvariantDisplayValue: "5/6"),
                new("1/8 Frame", "1/8 Frame", InvariantDisplayValue: "7/8"),
                new("1/16 Frame", "1/16 Frame", InvariantDisplayValue: "15/16"),
                new("1/32 Frame", "1/32 Frame", InvariantDisplayValue: "31/32"),
                new("1/60 Frame", "1/60 Frame", InvariantDisplayValue: "59/60"),
                new("Auto Frame Skip", "Auto Frame Skip", ResourceKey: SettingsChoiceConstants.Automatic)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.FrameSkip,
            ExplanationResourceKey: SettingsTextConstants.FrameSkipHelp
        ), new(
            OptionConstants.PushVideoBeforeAudio,
            "Push Video before Audio",
            null,
            EmulationMachineTab.Audio.ToString(),
            "disabled",
            "disabled",
            [
                new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.VideoBeforeAudio,
            ExplanationResourceKey: SettingsTextConstants.VideoBeforeAudioHelp
        ), new(
            OptionConstants.AdjustFrameRates,
            "Adjust Frame Rates",
            null,
            EmulationMachineTab.Video.ToString(),
            "enabled",
            "enabled",
            [
                new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled),
                new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.FrameRateAdjustment,
            ExplanationResourceKey: SettingsTextConstants.FrameRateAdjustmentHelp
        ), new(
            OptionConstants.AudioDesyncHack,
            "Audio Desync Hack",
            null,
            EmulationMachineTab.Audio.ToString(),
            "disabled",
            "disabled",
            [
                new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.AudioDesynchronization,
            ExplanationResourceKey: SettingsTextConstants.AudioDesynchronizationHelp
        ), new(
            OptionConstants.TextOff,
            "Text Off",
            null,
            EmulationMachineTab.Video.ToString(),
            "disabled",
            "disabled",
            [
                new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.HideText,
            ExplanationResourceKey: SettingsTextConstants.HideTextHelp
        ), new(
            OptionConstants.GrpOff,
            "Grp Off",
            null,
            EmulationMachineTab.Video.ToString(),
            "disabled",
            "disabled",
            [
                new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.HideGraphics,
            ExplanationResourceKey: SettingsTextConstants.HideGraphicsHelp
        ), new(
            OptionConstants.SpOff,
            "SP/BG Off",
            null,
            EmulationMachineTab.Video.ToString(),
            "disabled",
            "disabled",
            [
                new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.HideSprites,
            ExplanationResourceKey: SettingsTextConstants.HideSpritesHelp
        ), });
}