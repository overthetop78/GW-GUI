using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.MachineAdapters;

internal static class AtariMediaActivityScenarios
{
    public static void HatariOverlay()
    {
        const int width = 64;
        const int height = 12;
        const int pitch = width * 2 + 8;
        var pixels = new byte[pitch * height];
        PaintLed(pixels, width - 12, pitch);
        PaintLed(pixels, width - 36, pitch);
        var ledStates = new Dictionary<int, bool>();

        Assert.True(EmulationMediaActivityFunctions.CaptureHatariOverlay(
            pixels, width, height, pitch, EmulationPixelFormat.Rgb565, ledStates));
        Assert.True(ledStates[0]);
        Assert.False(ledStates[1]);
        Assert.True(ledStates[2]);

        var activity = EmulationMediaActivityFunctions.FromLedStates(AtariEmulator.Hatari, ledStates);
        Assert.True(activity[EmulationMediaSlot.Floppy0]);
        Assert.False(activity[EmulationMediaSlot.Floppy1]);
        Assert.True(activity[EmulationMediaSlot.HardDisk0]);

        Assert.True(EmulationMediaActivityFunctions.CaptureHatariOverlay(
            new byte[pitch * height], width, height, pitch, EmulationPixelFormat.Rgb565, ledStates));
        Assert.All(ledStates.Values, Assert.False);
    }

    public static void Atari800Overlay()
    {
        const int width = 336;
        const int height = 240;
        const int pitch = width * 2;
        var pixels = new byte[pitch * height];
        PaintGlyph(pixels, 326, 233, pitch, [0x00, 0x0c, 0x0a, 0x0a, 0x0a, 0x0c, 0x00]);
        PaintGlyph(pixels, 331, 233, pitch, [0x00, 0x0c, 0x02, 0x04, 0x08, 0x0e, 0x00]);
        PaintGlyph(pixels, 321, 233, pitch, [0x00, 0x04, 0x0a, 0x08, 0x0a, 0x04, 0x00]);
        var ledStates = new Dictionary<int, bool>();

        Assert.True(EmulationMediaActivityFunctions.CaptureAtari800Overlay(
            pixels, width, height, pitch, EmulationPixelFormat.Rgb565, ledStates));
        Assert.False(ledStates[0]);
        Assert.True(ledStates[1]);
        Assert.False(ledStates[2]);
        Assert.False(ledStates[3]);
        Assert.True(ledStates[4]);

        var activity = EmulationMediaActivityFunctions.FromLedStates(AtariEmulator.Atari800, ledStates);
        Assert.True(activity[EmulationMediaSlot.Floppy1]);
        Assert.True(activity[EmulationMediaSlot.Cassette0]);

        Assert.True(EmulationMediaActivityFunctions.CaptureAtari800Overlay(
            new byte[pitch * height], width, height, pitch, EmulationPixelFormat.Rgb565, ledStates));
        Assert.All(ledStates.Values, Assert.False);
    }

    private static void PaintLed(Span<byte> pixels, int centerX, int pitch)
    {
        for (var y = 7; y <= 9; y++)
        for (var x = centerX - 1; x <= centerX + 1; x++)
        {
            var offset = y * pitch + x * 2;
            pixels[offset] = 0xc0;
            pixels[offset + 1] = 0xfe;
        }
    }

    private static void PaintGlyph(Span<byte> pixels, int x, int y, int pitch, ReadOnlySpan<byte> rows)
    {
        const ushort light = 0x1234;
        for (var row = 0; row < rows.Length; row++)
        for (var column = 0; column < 5; column++)
        {
            var set = (rows[row] & (1 << (4 - column))) != 0;
            var value = set ? (ushort)0 : light;
            var offset = (y + row) * pitch + (x + column) * 2;
            pixels[offset] = (byte)value;
            pixels[offset + 1] = (byte)(value >> 8);
        }
    }
}
