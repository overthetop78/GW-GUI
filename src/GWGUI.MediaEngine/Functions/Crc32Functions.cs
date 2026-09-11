namespace GWGUI.MediaEngine.Functions;

/// <summary>Computes the standard reflected CRC-32 used by media container structures.</summary>
internal static class Crc32Functions
{
    private const uint Polynomial = 0xEDB88320;

    public static uint Compute(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = crc >> 1 ^ (Polynomial & (uint)-(int)(crc & 1));
        }
        return ~crc;
    }
}
