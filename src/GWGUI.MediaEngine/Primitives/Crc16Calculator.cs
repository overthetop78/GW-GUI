namespace GWGUI.MediaEngine.Primitives;

internal static class Crc16Calculator
{
    public static ushort Compute(IEnumerable<byte> values, ushort polynomial = 0x1021, ushort initial = 0xffff)
    {
        var crc = initial;
        foreach (var value in values) crc = Update(crc, value, polynomial);
        return crc;
    }

    public static ushort Update(ushort crc, byte value, ushort polynomial = 0x1021)
    {
        crc ^= (ushort)(value << 8);
        for (var bit = 0; bit < 8; bit++)
            crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ polynomial : crc << 1);
        return crc;
    }
}
