using GWGUI.Emulation.Sharp.Emulators.X1.Constants;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Dictionaries;
internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
            OptionConstants.Resolute,
            "Resolution",
            null,
            EmulationMachineTab.Video.ToString(),
            "LOW",
            "LOW",
            [
                new("LOW", "LOW", ResourceKey: SettingsChoiceConstants.Low),
                new("HIGH", "HIGH", ResourceKey: SettingsChoiceConstants.High)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.Resolution,
            ExplanationResourceKey: SettingsTextConstants.ResolutionHelp
        ), new(OptionConstants.Bootmedia,
        "Boot media",
        null,
        null,
        "2HD",
        "2HD",
        [new("2HD", "2HD"),
        new("2D", "2D")],
        IsVisible: false), new(OptionConstants.Romtype,
        "ROM type",
        null,
        null,
        "X1",
        "X1",
        [new("X1", "X1"),
        new("TURBO", "TURBO"),
        new("TURBOZ", "TURBOZ")],
        IsVisible: false), new(
            OptionConstants.Fps,
            "FPS",
            null,
            EmulationMachineTab.Video.ToString(),
            "AUTO",
            "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("60", "60", InvariantDisplayValue: "60 fps"),
                new("30", "30", InvariantDisplayValue: "30 fps"),
                new("20", "20", InvariantDisplayValue: "20 fps"),
                new("15", "15", InvariantDisplayValue: "15 fps")
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.FrameRate,
            ExplanationResourceKey: SettingsTextConstants.FrameRateHelp
        ), new(
            OptionConstants.Dispsync,
            "Disp Vsync",
            null,
            EmulationMachineTab.Video.ToString(),
            "OFF",
            "OFF",
            [
                new("OFF", "OFF", ResourceKey: SettingsChoiceConstants.Disabled),
                new("ON", "ON", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.VerticalSync,
            ExplanationResourceKey: SettingsTextConstants.VerticalSyncHelp
        ), new(
            OptionConstants.Raster,
            "Raster",
            null,
            EmulationMachineTab.Video.ToString(),
            "OFF",
            "OFF",
            [
                new("OFF", "OFF", ResourceKey: SettingsChoiceConstants.Disabled),
                new("ON", "ON", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.Raster,
            ExplanationResourceKey: SettingsTextConstants.RasterHelp
        ), new(
            OptionConstants.Nowait,
            "No wait",
            null,
            EmulationMachineTab.Cpu.ToString(),
            "OFF",
            "OFF",
            [
                new("OFF", "OFF", ResourceKey: SettingsChoiceConstants.Disabled),
                new("ON", "ON", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.UnlimitedSpeed,
            ExplanationResourceKey: SettingsTextConstants.UnlimitedSpeedHelp
        ), new(
            OptionConstants.BtnMode,
            "Joy Reverse",
            null,
            EmulationMachineTab.Controllers.ToString(),
            "OFF",
            "OFF",
            [
                new("OFF", "OFF", ResourceKey: SettingsChoiceConstants.Disabled),
                new("ON", "ON", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.ReverseJoystick,
            ExplanationResourceKey: SettingsTextConstants.ReverseJoystickHelp
        ), new(
            OptionConstants.BtnRapid,
            "Joy Rapid",
            null,
            EmulationMachineTab.Controllers.ToString(),
            "OFF",
            "OFF",
            [
                new("OFF", "OFF", ResourceKey: SettingsChoiceConstants.Disabled),
                new("ON", "ON", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.RapidFire,
            ExplanationResourceKey: SettingsTextConstants.RapidFireHelp
        ), new(
            OptionConstants.AudioRate,
            "Audio sampling rate",
            null,
            EmulationMachineTab.Audio.ToString(),
            "44100",
            "44100",
            [
                new("44100", "44100", InvariantDisplayValue: "44100 Hz"),
                new("22050", "22050", InvariantDisplayValue: "22050 Hz"),
                new("11025", "11025", InvariantDisplayValue: "11025 Hz")
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.AudioSampleRate,
            ExplanationResourceKey: SettingsTextConstants.AudioSampleRateHelp
        ), new(
            OptionConstants.Seeksnd,
            "Seek Sound",
            null,
            EmulationMachineTab.Audio.ToString(),
            "OFF",
            "OFF",
            [
                new("OFF", "OFF", ResourceKey: SettingsChoiceConstants.Disabled),
                new("ON", "ON", ResourceKey: SettingsChoiceConstants.Enabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.DiskSeekSound,
            ExplanationResourceKey: SettingsTextConstants.DiskSeekSoundHelp
        ), new(OptionConstants.Scanlines,
        "Scanlines in low-res",
        null,
        null,
        "OFF",
        "OFF",
        [new("OFF", "OFF"),
        new("ON", "ON")],
        IsVisible: false), new(OptionConstants.KeyMode,
        "Key mode",
        null,
        null,
        "Keyboard",
        "Keyboard",
        [new("Keyboard", "Keyboard"),
        new("JoyKey-1", "JoyKey-1"),
        new("JoyKey-2", "JoyKey-2"),
        new("Mouse-Key", "Mouse-Key")],
        IsVisible: false), new(
            OptionConstants.Fmboard,
            "FM Board",
            null,
            EmulationMachineTab.Audio.ToString(),
            "ON",
            "ON",
            [
                new("ON", "ON", ResourceKey: SettingsChoiceConstants.Enabled),
                new("OFF", "OFF", ResourceKey: SettingsChoiceConstants.Disabled)
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.FmBoard,
            ExplanationResourceKey: SettingsTextConstants.FmBoardHelp
        ), new(
            OptionConstants.AudioDelayms,
            "Audio buffer (ms)",
            null,
            null,
            "250",
            "250",
            [new("250", "250"), new("100", "100"), new("150", "150"), new("200", "200"), new("300", "300"), new("350", "350"), new("500", "500"), new("750", "750"), new("1000", "1000")],
            IsVisible: false
        ), new(
            OptionConstants.CpuClock,
            "Cpu clock (MHz)",
            null,
            EmulationMachineTab.Cpu.ToString(),
            "4",
            "4",
            [
                new("4", "4", InvariantDisplayValue: "4 MHz"),
                new("1", "1", InvariantDisplayValue: "1 MHz"),
                new("2", "2", InvariantDisplayValue: "2 MHz"),
                new("3", "3", InvariantDisplayValue: "3 MHz"),
                new("5", "5", InvariantDisplayValue: "5 MHz"),
                new("6", "6", InvariantDisplayValue: "6 MHz"),
                new("7", "7", InvariantDisplayValue: "7 MHz"),
                new("8", "8", InvariantDisplayValue: "8 MHz"),
                new("9", "9", InvariantDisplayValue: "9 MHz"),
                new("10", "10", InvariantDisplayValue: "10 MHz"),
                new("11", "11", InvariantDisplayValue: "11 MHz"),
                new("12", "12", InvariantDisplayValue: "12 MHz"),
                new("13", "13", InvariantDisplayValue: "13 MHz"),
                new("14", "14", InvariantDisplayValue: "14 MHz"),
                new("15", "15", InvariantDisplayValue: "15 MHz"),
                new("16", "16", InvariantDisplayValue: "16 MHz"),
                new("17", "17", InvariantDisplayValue: "17 MHz"),
                new("18", "18", InvariantDisplayValue: "18 MHz"),
                new("19", "19", InvariantDisplayValue: "19 MHz"),
                new("20", "20", InvariantDisplayValue: "20 MHz")
            ],
            IsVisible: true,
            ResourceKey: SettingsTextConstants.CpuSpeed,
            ExplanationResourceKey: SettingsTextConstants.CpuSpeedHelp
        ), });
}