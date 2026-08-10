using System.Text;

namespace GWGUI.MediaEngine.Encoding;

internal static class TrackEncoding
{
    public static ScpRevolution ToRevolution(IReadOnlyList<bool> bits, uint cellTicks, uint indexTimeTicks)
    {
        if (cellTicks == 0) throw new ArgumentOutOfRangeException(nameof(cellTicks));
        var intervals = new List<uint>();
        uint cells = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (!bit) continue;
            intervals.Add(checked(cells * cellTicks));
            cells = 0;
        }
        if (cells > 0) intervals.Add(checked(cells * cellTicks));
        return new(indexTimeTicks, (uint)intervals.Count, intervals);
    }

    public static List<bool> Bits() => [];

    public static void Raw(this List<bool> bits, params byte[] bytes)
    {
        foreach (var value in bytes)
            for (var bit = 7; bit >= 0; bit--)
                bits.Add((value & 1 << bit) != 0);
    }

    public static void RawHex(this List<bool> bits, string hexadecimal) => bits.Raw(Convert.FromHexString(hexadecimal));

    public static void RawBits(this List<bool> bits, string values)
    {
        foreach (var value in values) bits.Add(value == '1');
    }

    public static void DoubledCells(this List<bool> bits, IEnumerable<byte> bytes)
    {
        foreach (var value in bytes)
            for (var bit = 7; bit >= 0; bit--)
            {
                bits.Add(false);
                bits.Add((value & 1 << bit) != 0);
            }
    }

    public static void Mfm(this List<bool> bits, IEnumerable<byte> bytes, bool previousData = false)
    {
        var previous = previousData;
        foreach (var value in bytes)
            for (var bit = 7; bit >= 0; bit--)
            {
                var data = (value & 1 << bit) != 0;
                bits.Add(!previous && !data);
                bits.Add(data);
                previous = data;
            }
    }

    public static void Fm(this List<bool> bits, IEnumerable<byte> bytes)
    {
        foreach (var value in bytes)
            for (var bit = 7; bit >= 0; bit--)
            {
                bits.Add(true);
                bits.Add((value & 1 << bit) != 0);
            }
    }

    public static void DoubleFm(this List<bool> bits, IEnumerable<byte> bytes, bool reverse = false)
    {
        foreach (var source in bytes)
        {
            var value = reverse ? ReverseBits(source) : source;
            for (var bit = 7; bit >= 0; bit--)
            {
                bits.Add(false); bits.Add(true);
                bits.Add(false); bits.Add((value & 1 << bit) != 0);
            }
        }
    }

    public static void Gap(this List<bool> bits, int count, bool value = false)
    {
        for (var index = 0; index < count; index++) bits.Add(value || (index & 1) == 0);
    }

    public static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        throw new ArgumentException($"Unsupported sector size: {size} bytes.", nameof(size));
    }

    public static ushort Crc16(IEnumerable<byte> values, ushort polynomial = 0x1021, ushort initial = 0xffff)
        => Primitives.Crc16Calculator.Compute(values, polynomial, initial);

    public static byte[] WithCrc(IEnumerable<byte> values, ushort polynomial = 0x1021, ushort initial = 0xffff)
    {
        var result = values.ToList();
        var crc = Crc16(result, polynomial, initial);
        result.Add((byte)(crc >> 8)); result.Add((byte)crc);
        return result.ToArray();
    }

    public static byte RotatingChecksum(IEnumerable<byte> values)
    {
        byte checksum = 0;
        foreach (var value in values) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        return checksum;
    }

    public static byte ReverseBits(byte value)
        => Primitives.BitPrimitives.Reverse(value);
}
