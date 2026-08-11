using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Calcule le CRC32 défini par les conteneurs WOZ.</summary>
internal static class WozCrc32
{
    /// <summary>Calcule le CRC32 WOZ des octets fournis.</summary>
    /// <param name="data">Octets couverts par le CRC.</param>
    /// <returns>CRC32 utilisant le polynôme inversé du format WOZ.</returns>
    public static uint Compute(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++) crc = crc >> 1 ^ (WozFormat.Crc32Polynomial & (uint)-(int)(crc & 1));
        }
        return ~crc;
    }
}
