using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class EmulationMediaActivityFunctions
{
    private const int FloppyALed = 0;
    private const int FloppyBLed = 1;
    private const int HardDiskLed = 2;
    private const ushort HatariActiveLedColor = 0xfec0;
    private const int HatariLedCenterY = 8;
    private const int HatariLedSpacing = 12;
    private const int SampleRadius = 1;
    private const int MinimumMatchingSamples = 5;
    private const int Atari800SourceWidth = 384;
    private const int Atari800SourceHeight = 240;
    private const int Atari800IndicatorY = 233;
    private const int Atari800DiskX = 350;
    private const int Atari800DriveX = 355;
    private const int Atari800CassetteWithDiskX = 345;
    private static readonly byte[] Atari800DiskGlyph = [0x00, 0x0c, 0x0a, 0x0a, 0x0a, 0x0c, 0x00];
    private static readonly byte[] Atari800CassetteGlyph = [0x00, 0x04, 0x0a, 0x08, 0x0a, 0x04, 0x00];
    private static readonly byte[][] Atari800DriveGlyphs =
    [
        [0x00, 0x04, 0x0c, 0x04, 0x04, 0x04, 0x00],
        [0x00, 0x0c, 0x02, 0x04, 0x08, 0x0e, 0x00],
        [0x00, 0x0c, 0x02, 0x04, 0x02, 0x0c, 0x00],
        [0x00, 0x02, 0x06, 0x0a, 0x0e, 0x02, 0x00]
    ];

    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromRuntimeStatus(
        AtariRuntimeStatus status) => status.MediaActivity;

    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStates(AtariEmulator emulator,
        IReadOnlyDictionary<int, bool> ledStates) => emulator switch
    {
        AtariEmulator.Hatari => new Dictionary<EmulationMediaSlot, bool>
        {
            [EmulationMediaSlot.Floppy0] = ledStates.GetValueOrDefault(FloppyALed),
            [EmulationMediaSlot.Floppy1] = ledStates.GetValueOrDefault(FloppyBLed),
            [EmulationMediaSlot.HardDisk0] = ledStates.GetValueOrDefault(HardDiskLed)
        },
        AtariEmulator.Atari800 => new Dictionary<EmulationMediaSlot, bool>
        {
            [EmulationMediaSlot.Floppy0] = ledStates.GetValueOrDefault(0),
            [EmulationMediaSlot.Floppy1] = ledStates.GetValueOrDefault(1),
            [new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, 2)] = ledStates.GetValueOrDefault(2),
            [new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, 3)] = ledStates.GetValueOrDefault(3),
            [EmulationMediaSlot.Cassette0] = ledStates.GetValueOrDefault(4)
        },
        AtariEmulator.VirtualJaguar => new Dictionary<EmulationMediaSlot, bool>
        {
            [EmulationMediaSlot.Cd0] = ledStates.GetValueOrDefault(0)
        },
        _ => new Dictionary<EmulationMediaSlot, bool>()
    };

    internal static bool CaptureHatariOverlay(ReadOnlySpan<byte> pixels, int width, int height, int pitch,
        EmulationPixelFormat pixelFormat, IDictionary<int, bool> ledStates)
    {
        if (pixelFormat != EmulationPixelFormat.Rgb565 || width < 48 || height <= HatariLedCenterY + SampleRadius
            || pitch < width * sizeof(ushort) || pixels.Length < pitch * height)
            return false;

        ledStates[FloppyALed] = IsActive(pixels, width - HatariLedSpacing, pitch);
        ledStates[FloppyBLed] = IsActive(pixels, width - HatariLedSpacing * 2, pitch);
        ledStates[HardDiskLed] = IsActive(pixels, width - HatariLedSpacing * 3, pitch);
        return true;
    }

    internal static bool CaptureAtari800Overlay(ReadOnlySpan<byte> pixels, int width, int height, int pitch,
        EmulationPixelFormat pixelFormat, IDictionary<int, bool> ledStates)
    {
        var bytesPerPixel = pixelFormat == EmulationPixelFormat.Xrgb8888 ? 4 : 2;
        if (width <= 0 || height <= 0 || pitch < width * bytesPerPixel || pixels.Length < pitch * height)
            return false;
        for (var drive = 0; drive < Atari800DriveGlyphs.Length; drive++) ledStates[drive] = false;
        ledStates[4] = false;

        var copiedWidth = Math.Min(width, Atari800SourceWidth);
        var copiedHeight = Math.Min(height, Atari800SourceHeight);
        var sourceX = (Atari800SourceWidth - copiedWidth) / 2;
        var sourceY = (Atari800SourceHeight - copiedHeight) / 2;
        var destinationX = (width - copiedWidth) / 2;
        var destinationY = (height - copiedHeight) / 2;
        var y = Atari800IndicatorY - sourceY + destinationY;
        var diskX = Atari800DiskX - sourceX + destinationX;
        var driveX = Atari800DriveX - sourceX + destinationX;
        var cassetteWithoutDiskX = Atari800DriveX - sourceX + destinationX;
        var cassetteWithDiskX = Atari800CassetteWithDiskX - sourceX + destinationX;

        var diskVisible = MatchesGlyph(pixels, diskX, y, pitch, pixelFormat, Atari800DiskGlyph);
        if (diskVisible)
            for (var drive = 0; drive < Atari800DriveGlyphs.Length; drive++)
                if (MatchesGlyph(pixels, driveX, y, pitch, pixelFormat, Atari800DriveGlyphs[drive]))
                {
                    ledStates[drive] = true;
                    break;
                }
        ledStates[4] = MatchesGlyph(pixels, diskVisible ? cassetteWithDiskX : cassetteWithoutDiskX,
            y, pitch, pixelFormat, Atari800CassetteGlyph);
        return true;
    }

    private static bool MatchesGlyph(ReadOnlySpan<byte> pixels, int x, int y, int pitch,
        EmulationPixelFormat pixelFormat, ReadOnlySpan<byte> rows)
    {
        const int glyphWidth = 5;
        if (x < 0 || y < 0 || x + glyphWidth > pitch / (pixelFormat == EmulationPixelFormat.Xrgb8888 ? 4 : 2)
            || y + rows.Length > pixels.Length / pitch) return false;
        var light = Pixel(pixels, x, y, pitch, pixelFormat);
        uint? dark = null;
        for (var row = 0; row < rows.Length; row++)
        for (var column = 0; column < glyphWidth; column++)
        {
            var set = (rows[row] & (1 << (glyphWidth - 1 - column))) != 0;
            var value = Pixel(pixels, x + column, y + row, pitch, pixelFormat);
            if (!set)
            {
                if (value != light) return false;
            }
            else if (dark is null) dark = value;
            else if (value != dark) return false;
        }
        return dark is { } darkValue && darkValue != light;
    }

    private static uint Pixel(ReadOnlySpan<byte> pixels, int x, int y, int pitch, EmulationPixelFormat format)
    {
        var offset = y * pitch + x * (format == EmulationPixelFormat.Xrgb8888 ? 4 : 2);
        return format == EmulationPixelFormat.Xrgb8888
            ? (uint)(pixels[offset] | pixels[offset + 1] << 8 | pixels[offset + 2] << 16 | pixels[offset + 3] << 24)
            : (uint)(pixels[offset] | pixels[offset + 1] << 8);
    }

    private static bool IsActive(ReadOnlySpan<byte> pixels, int centerX, int pitch)
    {
        var matchingSamples = 0;
        for (var y = HatariLedCenterY - SampleRadius; y <= HatariLedCenterY + SampleRadius; y++)
        for (var x = centerX - SampleRadius; x <= centerX + SampleRadius; x++)
        {
            var offset = y * pitch + x * sizeof(ushort);
            var value = (ushort)(pixels[offset] | pixels[offset + 1] << 8);
            if (value == HatariActiveLedColor) matchingSamples++;
        }
        return matchingSamples >= MinimumMatchingSamples;
    }
}
