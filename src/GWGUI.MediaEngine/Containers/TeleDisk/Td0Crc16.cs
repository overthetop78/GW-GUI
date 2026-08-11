using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

internal static class Td0Crc16
{
    private const ushort Polynomial = 0xA097;

    public static ushort Compute(ReadOnlySpan<byte> data, ushort initial = 0)
    {
        var crc = initial;
        foreach (var value in data) crc = Crc16Calculator.Update(crc, value, Polynomial);
        return crc;
    }
}
