namespace GWGUI.MediaEngine.Decoding;

internal static class AppleBitLatch
{
    public static byte[]? TryReadBytes(bool[] bits, ref int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (offset + 8 > bits.Length) return null;
            byte value = 0;
            for (var bit = 0; bit < 8; bit++)
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));

            while ((value & 0x80) == 0)
            {
                if (offset >= bits.Length) return null;
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));
            }

            result[index] = value;
        }

        return result;
    }
}
