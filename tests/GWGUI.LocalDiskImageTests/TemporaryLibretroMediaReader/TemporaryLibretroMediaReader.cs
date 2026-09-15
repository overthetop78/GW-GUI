#if TEMPORARY_LIBRETRO_MEDIA_READER
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.MediaAudit;

internal static class TemporaryLibretroMediaReader
{
    private const uint SystemRamId = 2;
    private static readonly int[] CaptureFrames = [60, 180, 360, 600, 900, 1200, 1800];

    public static int Main(string[] args)
    {
        var imagePath = Path.GetFullPath(Required(args, "--image"));
        var outputDirectory = Path.GetFullPath(Required(args, "--output"));
        var corePath = Path.GetFullPath(Required(args, "--core"));
        var configurationPath = Path.GetFullPath(Required(args, "--configuration"));
        var sessionDirectory = Path.Combine(outputDirectory, "libretro-session");
        Directory.CreateDirectory(outputDirectory);
        Directory.CreateDirectory(sessionDirectory);

        var configuration = JsonSerializer.Deserialize<AtariMachineConfiguration>(
            File.ReadAllText(configurationPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException($"Configuration Atari invalide : {configurationPath}");
        configuration = configuration with
        {
            AudioEnabled = false,
            Media =
            [
                new AtariMediaConfiguration(imagePath, AtariMediaCategory.Floppy,
                    EmulationMediaSlot.Floppy0, IsReadOnly: true)
            ]
        };
        var presses = ParsePresses(args);
        var captures = new List<object>();
        AtariExternalCore? core = null;

        try
        {
            core = new AtariExternalCore(corePath, AtariEmulator.Atari800);
            core.Initialize(configuration, sessionDirectory);
            for (var frame = 1; frame <= CaptureFrames[^1]; frame++)
            {
                core.SetInput(presses.TryGetValue(frame, out var key)
                    ? WithKey(key)
                    : EmulationInputSnapshot.Empty);
                core.RunFrame();
                if (!CaptureFrames.Contains(frame)) continue;

                var framePath = Path.Combine(outputDirectory, $"libretro-frame-{frame:D4}.bmp");
                if (core.LatestVideoFrame is { } video)
                    WriteBitmap(framePath, video);
                var ram = ReadSystemRam(core);
                var ramPath = Path.Combine(outputDirectory, $"libretro-ram-{frame:D4}.bin");
                if (ram.Length > 0) File.WriteAllBytes(ramPath, ram);
                captures.Add(new
                {
                    Frame = frame,
                    Video = core.LatestVideoFrame is { } captured
                        ? new
                        {
                            captured.Width,
                            captured.Height,
                            captured.Pitch,
                            PixelFormat = captured.PixelFormat.ToString(),
                            Sha256 = Sha256(captured.Pixels.Span)
                        }
                        : null,
                    Ram = ram.Length == 0
                        ? null
                        : new
                        {
                            Length = ram.Length,
                            Sha256 = Sha256(ram),
                            Strings = ReadStrings(ram)
                        },
                    Diagnostics = core.Diagnostics.ToArray(),
                    Leds = core.LedStates
                });
            }
            File.WriteAllText(Path.Combine(outputDirectory, "libretro-observation.json"),
                JsonSerializer.Serialize(new
                {
                    Image = imagePath,
                    Core = corePath,
                    Configuration = configurationPath,
                    Presses = presses.Select(item => new { Frame = item.Key, Key = item.Value.ToString() }),
                    Captures = captures
                }, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        finally
        {
            try
            {
                core?.Stop();
            }
            finally
            {
                try
                {
                    core?.Dispose();
                }
                finally
                {
                    if (Directory.Exists(sessionDirectory)) Directory.Delete(sessionDirectory, true);
                }
            }
        }
    }

    private static string Required(IReadOnlyList<string> args, string name)
    {
        for (var index = 0; index < args.Count - 1; index++)
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
                return args[index + 1];
        throw new ArgumentException($"Argument requis : {name}");
    }

    private static SortedDictionary<int, EmulationKey> ParsePresses(IReadOnlyList<string> args)
    {
        var result = new SortedDictionary<int, EmulationKey>();
        foreach (var argument in args.Where(value => value.StartsWith("--press=", StringComparison.OrdinalIgnoreCase)))
        {
            var parts = argument[8..].Split(':', 2);
            if (parts.Length != 2 || !int.TryParse(parts[0], out var frame)
                || !Enum.TryParse<EmulationKey>(parts[1], true, out var key))
                throw new ArgumentException($"Touche invalide : {argument}");
            result[frame] = key;
        }
        return result;
    }

    private static EmulationInputSnapshot WithKey(EmulationKey key) =>
        EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { key } };

    private static byte[] ReadSystemRam(AtariExternalCore core)
    {
        var field = typeof(AtariExternalCore).GetField("_exports", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(AtariExternalCore).FullName, "_exports");
        var exports = (AtariExternalCoreExports?)field.GetValue(core)
            ?? throw new InvalidOperationException("Le cœur Atari n'est pas initialisé.");
        var size = exports.GetMemorySize(SystemRamId);
        var pointer = exports.GetMemoryData(SystemRamId);
        if (pointer == nint.Zero || size == nuint.Zero) return [];
        var ram = new byte[checked((int)size)];
        Marshal.Copy(pointer, ram, 0, ram.Length);
        return ram;
    }

    private static IReadOnlyList<string> ReadStrings(ReadOnlySpan<byte> data)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        foreach (var value in data)
        {
            var character = value is >= 0x20 and <= 0x7e ? (char)value : '\0';
            if (character != '\0')
            {
                current.Append(character);
                continue;
            }
            if (current.Length >= 5) result.Add(current.ToString());
            current.Clear();
        }
        if (current.Length >= 5) result.Add(current.ToString());
        return result.Distinct(StringComparer.Ordinal).Take(400).ToArray();
    }

    private static string Sha256(ReadOnlySpan<byte> data) => Convert.ToHexString(SHA256.HashData(data));

    private static void WriteBitmap(string path, VideoFrame frame)
    {
        var rowSize = checked(frame.Width * 4);
        var pixelsSize = checked(rowSize * frame.Height);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(54 + pixelsSize);
        writer.Write(0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(frame.Width);
        writer.Write(-frame.Height);
        writer.Write((short)1);
        writer.Write((short)32);
        writer.Write(0);
        writer.Write(pixelsSize);
        writer.Write(2835);
        writer.Write(2835);
        writer.Write(0);
        writer.Write(0);

        var source = frame.Pixels.Span;
        for (var y = 0; y < frame.Height; y++)
        {
            var row = source.Slice(y * frame.Pitch, frame.Pitch);
            for (var x = 0; x < frame.Width; x++)
            {
                var (red, green, blue) = ReadPixel(row, x, frame.PixelFormat);
                writer.Write(blue);
                writer.Write(green);
                writer.Write(red);
                writer.Write((byte)0xff);
            }
        }
    }

    private static (byte Red, byte Green, byte Blue) ReadPixel(
        ReadOnlySpan<byte> row, int x, EmulationPixelFormat format)
    {
        if (format == EmulationPixelFormat.Xrgb8888)
        {
            var offset = x * 4;
            return (row[offset + 2], row[offset + 1], row[offset]);
        }

        var packed = BitConverter.ToUInt16(row.Slice(x * 2, 2));
        return format == EmulationPixelFormat.Rgb565
            ? ((byte)(((packed >> 11) & 0x1f) * 255 / 31),
                (byte)(((packed >> 5) & 0x3f) * 255 / 63),
                (byte)((packed & 0x1f) * 255 / 31))
            : ((byte)(((packed >> 10) & 0x1f) * 255 / 31),
                (byte)(((packed >> 5) & 0x1f) * 255 / 31),
                (byte)((packed & 0x1f) * 255 / 31));
    }
}
#endif
