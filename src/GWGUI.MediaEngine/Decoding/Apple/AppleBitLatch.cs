using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Apple;

internal static class AppleBitLatch
{
    private const byte SynchronizedByteMask = 0x80;

    public static byte[]? TryReadBytes(IReadOnlyList<bool> bits, ref int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if ((uint)offset > (uint)bits.Count) throw new ArgumentOutOfRangeException(nameof(offset));
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (offset + BitPrimitives.BitsPerByte > bits.Count) return null;
            byte value = 0;
            for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++)
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));

            while ((value & SynchronizedByteMask) == 0)
            {
                if (offset >= bits.Count) return null;
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));
            }

            result[index] = value;
        }

        return result;
    }
}
