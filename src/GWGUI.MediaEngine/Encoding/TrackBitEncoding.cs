using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Produit les représentations binaires compactes des octets encodés en FM ou en MFM.</summary>
internal static class TrackBitEncoding
{
    /// <summary>Encode des octets en MFM puis compacte les cellules obtenues en octets.</summary>
    /// <param name="data">Octets à encoder.</param>
    /// <returns>Cellules MFM compactées, bit de poids fort en premier.</returns>
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

    /// <summary>Encode des octets en FM puis compacte les cellules obtenues en octets.</summary>
    /// <param name="data">Octets à encoder.</param>
    /// <returns>Cellules FM compactées, bit de poids fort en premier.</returns>
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

    /// <summary>Compacte une suite de cellules binaires dans un tableau d'octets.</summary>
    /// <param name="bits">Cellules à compacter, bit de poids fort en premier.</param>
    /// <returns>Octets contenant les cellules compactées.</returns>
    private static byte[] Pack(IReadOnlyList<bool> bits)
    {
        var bytes = new byte[(bits.Count + BitPrimitives.BitsPerByte - 1) / BitPrimitives.BitsPerByte];
        for (var index = 0; index < bits.Count; index++)
        {
            if (bits[index]) bytes[index / BitPrimitives.BitsPerByte] |= (byte)(1 << (BitPrimitives.BitsPerByte - 1 - index % BitPrimitives.BitsPerByte));
        }

        return bytes;
    }
}
