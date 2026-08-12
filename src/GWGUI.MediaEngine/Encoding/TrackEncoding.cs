using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

internal static class TrackEncoding
{
    public static FluxRevolution ToRevolution(IReadOnlyList<bool> bits, uint cellTicks, uint indexTimeTicks)
    {
        if (cellTicks == 0) throw TrackEncodingExceptions.ZeroBitCell(cellTicks);
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
        return new(indexTimeTicks, intervals);
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
            var value = reverse ? Primitives.BitPrimitives.ReverseBits(source) : source;
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
        if (SectorSizeCode.TryFromByteCount(size, out var code)) return code;
        throw TrackEncodingExceptions.UnsupportedSectorSize(size);
    }

    public static byte[] WithCrc(IEnumerable<byte> values, ushort polynomial = Primitives.Crc16Calculator.CcittPolynomial, ushort initial = Primitives.Crc16Calculator.AllBitsSetInitialValue)
    {
        var result = values.ToList();
        var crc = Primitives.Crc16Calculator.Compute(result, polynomial, initial);
        result.Add((byte)(crc >> Primitives.BitPrimitives.BitsPerByte)); result.Add((byte)crc);
        return result.ToArray();
    }

    public static byte RotatingChecksum(IEnumerable<byte> values)
    {
        byte checksum = 0;
        foreach (var value in values) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        return checksum;
    }
}
