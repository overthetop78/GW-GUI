using GWGUI.Emulation.Sharp.Emulators.X1.Constants;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Dictionaries;
internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(OptionConstants.Resolute, "Resolution", null, EmulationMachineTab.Video.ToString(), "LOW", "LOW", [new("LOW", "LOW"), new("HIGH", "HIGH")], IsVisible: true), new(OptionConstants.Bootmedia, "Boot media", null, null, "2HD", "2HD", [new("2HD", "2HD"), new("2D", "2D")], IsVisible: false), new(OptionConstants.Romtype, "ROM type", null, null, "X1", "X1", [new("X1", "X1"), new("TURBO", "TURBO"), new("TURBOZ", "TURBOZ")], IsVisible: false), new(OptionConstants.Fps, "FPS", null, EmulationMachineTab.Video.ToString(), "AUTO", "AUTO", [new("AUTO", "AUTO"), new("60", "60"), new("30", "30"), new("20", "20"), new("15", "15")], IsVisible: true), new(OptionConstants.Dispsync, "Disp Vsync", null, EmulationMachineTab.Video.ToString(), "OFF", "OFF", [new("OFF", "OFF"), new("ON", "ON")], IsVisible: true), new(OptionConstants.Raster, "Raster", null, EmulationMachineTab.Video.ToString(), "OFF", "OFF", [new("OFF", "OFF"), new("ON", "ON")], IsVisible: true), new(OptionConstants.Nowait, "No wait", null, EmulationMachineTab.Cpu.ToString(), "OFF", "OFF", [new("OFF", "OFF"), new("ON", "ON")], IsVisible: true), new(OptionConstants.BtnMode, "Joy Reverse", null, EmulationMachineTab.Controllers.ToString(), "OFF", "OFF", [new("OFF", "OFF"), new("ON", "ON")], IsVisible: true), new(OptionConstants.BtnRapid, "Joy Rapid", null, EmulationMachineTab.Controllers.ToString(), "OFF", "OFF", [new("OFF", "OFF"), new("ON", "ON")], IsVisible: true), new(OptionConstants.AudioRate, "Audio sampling rate", null, EmulationMachineTab.Audio.ToString(), "44100", "44100", [new("44100", "44100"), new("22050", "22050"), new("11025", "11025")], IsVisible: true), new(OptionConstants.Seeksnd, "Seek Sound", null, EmulationMachineTab.Audio.ToString(), "OFF", "OFF", [new("OFF", "OFF"), new("ON", "ON")], IsVisible: true), new(OptionConstants.Scanlines, "Scanlines in low-res", null, null, "OFF", "OFF", [new("OFF", "OFF"), new("ON", "ON")], IsVisible: false), new(OptionConstants.KeyMode, "Key mode", null, null, "Keyboard", "Keyboard", [new("Keyboard", "Keyboard"), new("JoyKey-1", "JoyKey-1"), new("JoyKey-2", "JoyKey-2"), new("Mouse-Key", "Mouse-Key")], IsVisible: false), new(OptionConstants.Fmboard, "FM Board", null, EmulationMachineTab.Audio.ToString(), "ON", "ON", [new("ON", "ON"), new("OFF", "OFF")], IsVisible: true), new(
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
                new("4", "4"),
                new("1", "1"),
                new("2", "2"),
                new("3", "3"),
                new("5", "5"),
                new("6", "6"),
                new("7", "7"),
                new("8", "8"),
                new("9", "9"),
                new("10", "10"),
                new("11", "11"),
                new("12", "12"),
                new("13", "13"),
                new("14", "14"),
                new("15", "15"),
                new("16", "16"),
                new("17", "17"),
                new("18", "18"),
                new("19", "19"),
                new("20", "20")
            ],
            IsVisible: true
        ), });
}