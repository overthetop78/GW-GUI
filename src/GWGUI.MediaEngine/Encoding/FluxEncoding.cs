using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

internal static class FluxEncoding
{
    public static byte[] EncodeMfm(params byte[] data)
    {
        var bits = new List<bool>(data.Length * 16);
        var previousData = false;
        foreach (var value in data)
        {
            for (var bit = 7; bit >= 0; bit--)
            {
                var current = (value & (1 << bit)) != 0;
                bits.Add(!previousData && !current);
                bits.Add(current);
                previousData = current;
            }
        }

        return Pack(bits);
    }

    public static byte[] EncodeFm(params byte[] data)
    {
        var bits = new List<bool>(data.Length * 16);
        foreach (var value in data)
        {
            for (var bit = 7; bit >= 0; bit--)
            {
                bits.Add(true);
                bits.Add((value & (1 << bit)) != 0);
            }
        }

        return Pack(bits);
    }

    private static byte[] Pack(IReadOnlyList<bool> bits)
    {
        var bytes = new byte[(bits.Count + BitPrimitives.BitsPerByte - 1) / BitPrimitives.BitsPerByte];
        for (var index = 0; index < bits.Count; index++)
        {
            if (bits[index])
                bytes[index / BitPrimitives.BitsPerByte] |= (byte)(1 << (BitPrimitives.BitsPerByte - 1 - index % BitPrimitives.BitsPerByte));
        }

        return bytes;
    }
}
