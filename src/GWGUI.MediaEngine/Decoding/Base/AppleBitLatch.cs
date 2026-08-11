using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

internal static class AppleBitLatch
{
    public static byte[]? TryReadBytes(IReadOnlyList<bool> bits, ref int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (offset + BitPrimitives.BitsPerByte > bits.Count) return null;
            byte value = 0;
            for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++)
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));

            while ((value & 0x80) == 0)
            {
                if (offset >= bits.Count) return null;
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));
            }

            result[index] = value;
        }

        return result;
    }
}
