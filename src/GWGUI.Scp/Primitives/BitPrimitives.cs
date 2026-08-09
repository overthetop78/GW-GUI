namespace GWGUI.Scp.Primitives;

internal static class BitPrimitives
{
    public static byte Reverse(byte value)
    {
        var result = 0;
        for (var bit = 0; bit < 8; bit++) result = result << 1 | value >> bit & 1;
        return (byte)result;
    }
}
