namespace GWGUI.MediaEngine.Functions;

/// <summary>Calculates CRC-32C checksums with the Castagnoli polynomial.</summary>
internal static class Crc32CFunctions
{
    private const uint ReversedPolynomial = 0x82F63B78;
    private static readonly uint[] Table = CreateTable();

    public static uint Compute(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in data)
            crc = Table[(crc ^ value) & byte.MaxValue] ^ (crc >> 8);
        return ~crc;
    }

    private static uint[] CreateTable()
    {
        var table = new uint[256];
        for (uint index = 0; index < table.Length; index++)
        {
            var value = index;
            for (var bit = 0; bit < 8; bit++)
                value = (value >> 1) ^ ((value & 1) == 0 ? 0 : ReversedPolynomial);
            table[index] = value;
        }
        return table;
    }
}
