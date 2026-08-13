namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

internal static class GreaseweazleFluxStreamEncoder
{
    private const byte EndOfStream = 0;
    private const byte Escape = 255;
    private const byte Space = 2;
    private const byte Astable = 3;

    public static byte[] Encode(ReadOnlySpan<uint> intervals, uint sampleFrequency)
    {
        if (sampleFrequency == 0) throw new ArgumentOutOfRangeException(nameof(sampleFrequency));

        var bytes = new List<byte>(intervals.Length * 2 + 16);
        var noFluxThreshold = checked((uint)Math.Round(150e-6 * sampleFrequency));
        var noFluxPeriod = checked((uint)Math.Round(1.25e-6 * sampleFrequency));
        foreach (var interval in intervals) WriteInterval(bytes, interval, noFluxThreshold, noFluxPeriod);

        WriteInterval(bytes, checked((uint)Math.Round(100e-6 * sampleFrequency)), noFluxThreshold, noFluxPeriod);
        bytes.Add(EndOfStream);
        return [.. bytes];
    }

    private static void WriteInterval(List<byte> bytes, uint value, uint noFluxThreshold, uint noFluxPeriod)
    {
        if (value == 0) return;
        if (value < 250)
        {
            bytes.Add((byte)value);
            return;
        }

        if (value > noFluxThreshold)
        {
            WriteOpcode(bytes, Space, value);
            WriteOpcode(bytes, Astable, noFluxPeriod);
            return;
        }

        var high = (value - 250) / 255;
        if (high < 5)
        {
            bytes.Add((byte)(250 + high));
            bytes.Add((byte)(1 + (value - 250) % 255));
            return;
        }

        WriteOpcode(bytes, Space, value - 249);
        bytes.Add(249);
    }

    private static void WriteOpcode(List<byte> bytes, byte opcode, uint value)
    {
        if (value >= 1u << 28) throw new ArgumentOutOfRangeException(nameof(value));
        bytes.Add(Escape);
        bytes.Add(opcode);
        bytes.Add((byte)(1 | (value << 1) & 255));
        bytes.Add((byte)(1 | (value >> 6) & 255));
        bytes.Add((byte)(1 | (value >> 13) & 255));
        bytes.Add((byte)(1 | (value >> 20) & 255));
    }
}
