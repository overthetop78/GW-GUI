namespace GWGUI.MediaEngine.Primitives;

internal static class Crc16Calculator
{
    public const ushort CcittPolynomial = 0x1021;
    public const ushort IbmPolynomial = 0x8005;
    public const ushort AllBitsSetInitialValue = 0xFFFF;
    public const ushort ZeroInitialValue = 0x0000;
    public const ushort HighBitMask = 0x8000;
    public const int ByteShift = BitPrimitives.BitsPerByte;

    public static ushort Compute(IEnumerable<byte> values, ushort polynomial = CcittPolynomial, ushort initial = AllBitsSetInitialValue)
    {
        ArgumentNullException.ThrowIfNull(values);
        var crc = initial;
        foreach (var value in values) crc = Update(crc, value, polynomial);
        return crc;
    }

    public static ushort Update(ushort crc, byte value, ushort polynomial = CcittPolynomial)
    {
        crc ^= (ushort)(value << ByteShift);
        for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++) crc = (ushort)((crc & HighBitMask) != 0 ? (crc << 1) ^ polynomial : crc << 1);
        return crc;
    }
}
