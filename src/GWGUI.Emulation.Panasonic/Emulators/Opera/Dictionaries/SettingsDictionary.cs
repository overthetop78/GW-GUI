using System.Globalization;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Dictionaries;
internal static class SettingsDictionary
{
    internal static IReadOnlyList<EmulationSettingsChoice> CpuScales { get; } = new (double Scale, string Token)[]
    {
        (0.5, "0.5x ( 6.25Mhz)"),
        (0.6, "0.6x ( 7.50Mhz)"),
        (0.7, "0.7x ( 8.75Mhz)"),
        (0.8, "0.8x (10.00Mhz)"),
        (0.9, "0.9x (11.25Mhz)"),
        (1, "1.0x (12.50Mhz)"),
        (1.1, "1.1x (13.75Mhz)"),
        (1.2, "1.2x (15.00Mhz)"),
        (1.5, "1.5x (18.75Mhz)"),
        (1.6, "1.6x (20.00Mhz)"),
        (1.8, "1.8x (22.50Mhz)"),
        (2, "2.0x (25.00Mhz)"),
        (3, "3.0x (37.50Mhz)"),
        (4, "4.0x (50.00Mhz)")
    }.Select(item => new EmulationSettingsChoice(item.Token, string.Empty, $"{item.Scale.ToString("0.0#", CultureInfo.CurrentCulture)}× · {(item.Scale * 12.5).ToString("0.##", CultureInfo.CurrentCulture)} MHz")).ToArray();
    internal static IReadOnlyList<EmulationSettingsChoice> Memory { get; } = new (string Token, int Ram, int Vram)[]
    {
        ("21", 2, 1),
        ("22", 2, 2),
        ("41", 4, 1),
        ("42", 4, 2),
        ("81", 8, 1),
        ("82", 8, 2),
        ("E2", 14, 2),
        ("F1", 15, 1)
    }.Select(item => new EmulationSettingsChoice(item.Token, string.Empty, $"{item.Ram} MiB RAM + {item.Vram} MiB VRAM")).ToArray();
    internal static IReadOnlyList<EmulationSettingsChoice> Modes { get; } = new (string Token, EmulationVideoStandard Standard, int Frequency, int Width, int Height)[]
    {
        ("ntsc", EmulationVideoStandard.Ntsc, 60, 320, 240),
        ("pal1", EmulationVideoStandard.Pal, 50, 320, 288),
        ("pal2", EmulationVideoStandard.Pal, 50, 384, 288)
    }.Select(item => new EmulationSettingsChoice(item.Token, string.Empty, $"{EmulationVideoFunctions.FormatTiming(item.Standard, item.Frequency)} · {item.Width} × {item.Height}")).ToArray();
    internal static IReadOnlyList<EmulationSettingsChoice> PixelFormats { get; } = [new("0RGB1555", string.Empty, "RGB1555"), new("RGB565", string.Empty, "RGB565"), new("XRGB8888", string.Empty, "XRGB8888")];
    internal static IReadOnlyList<EmulationSettingsChoice> MatrixModes { get; } = [new("hardware", "Emulation.Panasonic.Value.Hardware"), new("software", "Emulation.Panasonic.Value.Software")];
    internal static IReadOnlyList<EmulationSettingsChoice> SaveModes { get; } = [new("per game", "Emulation.Panasonic.Value.PerGame"), new("shared", "Emulation.Panasonic.Value.Shared")];
    internal static IReadOnlyList<EmulationSettingsChoice> CdSpeeds { get; } = new[]
    {
        new EmulationSettingsChoice("unbounded", "Emulation.Panasonic.Value.Unlimited")
    }.Concat(new[] { 1, 2, 4, 8, 16, 24, 32 }.Select(speed => new EmulationSettingsChoice(speed + "x", string.Empty, $"{speed}×"))).ToArray();
    internal static IReadOnlyList<EmulationSettingsChoice> Seeds { get; } = new[]
    {
        new EmulationSettingsChoice("random", "Emulation.Panasonic.Value.Random")
    }.Concat(new[] { "0x00000000", "0x00000001", "0xdeadbeef", "0xcafebabe", "0xfeedface", "0xbaadf00d", "0x8badf00d", "0xdeadc0de", "0xc001d00d", "0x0badf00d", "0x74726170", "0x65786974" }.Select(value => new EmulationSettingsChoice(value, string.Empty, value))).ToArray();
    internal static IReadOnlyList<EmulationSettingsChoice> Versions { get; } = Numbers(10);
    internal static IReadOnlyList<EmulationSettingsChoice> DeviceCounts { get; } = Numbers(ControllerConstants.PortCount + 1);

    private static IReadOnlyList<EmulationSettingsChoice> Numbers(int count) => Enumerable.Range(0, count).Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, value.ToString(CultureInfo.CurrentCulture))).ToArray();
}