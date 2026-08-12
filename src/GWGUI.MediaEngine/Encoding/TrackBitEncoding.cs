using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Fournit les primitives d'écriture et de compactage des cellules binaires d'une piste.</summary>
internal static class TrackBitEncoding
{
    /// <summary>Nombre de cellules FM ou MFM produites pour un octet source.</summary>
    private const int EncodedCellCountPerByte = BitPrimitives.BitsPerByte * 2;

    /// <summary>Crée un tampon de cellules binaires vide.</summary>
    /// <returns>Nouveau tampon modifiable.</returns>
    public static List<bool> Bits() => [];

    /// <summary>Ajoute les bits de chaque octet, du poids fort au poids faible.</summary>
    /// <param name="bits">Tampon recevant les cellules.</param>
    /// <param name="bytes">Octets à écrire.</param>
    public static void Raw(this List<bool> bits, params byte[] bytes)
    {
        foreach (var value in bytes)
        {
            for (var bit = BitPrimitives.BitsPerByte - 1; bit >= 0; bit--) bits.Add((value & (1 << bit)) != 0);
        }
    }

    /// <summary>Convertit une chaîne hexadécimale avec la plateforme puis écrit les octets obtenus.</summary>
    /// <param name="bits">Tampon recevant les cellules.</param>
    /// <param name="hexadecimal">Chaîne contenant un nombre pair de chiffres hexadécimaux.</param>
    /// <exception cref="FormatException">La chaîne n'est pas une représentation hexadécimale valide.</exception>
    public static void RawHex(this List<bool> bits, string hexadecimal) => bits.Raw(Convert.FromHexString(hexadecimal));

    /// <summary>Ajoute une suite textuelle de cellules binaires.</summary>
    /// <param name="bits">Tampon recevant les cellules.</param>
    /// <param name="values">Chaîne composée uniquement de <c>0</c> et de <c>1</c>.</param>
    /// <exception cref="ArgumentException">Un caractère différent de <c>0</c> ou <c>1</c> est rencontré.</exception>
    public static void RawBits(this List<bool> bits, string values)
    {
        for (var index = 0; index < values.Length; index++)
        {
            bits.Add(values[index] switch { '0' => false, '1' => true, _ => throw TrackEncodingExceptions.InvalidBinaryCharacter(values[index], index) });
        }
    }

    /// <summary>Ajoute des octets encodés en MFM.</summary>
    /// <param name="bits">Tampon recevant les cellules.</param>
    /// <param name="bytes">Octets à encoder.</param>
    /// <param name="previousData">État de la cellule de données précédant le premier octet.</param>
    /// <remarks>L'état de données est propagé entre tous les octets d'un même appel.</remarks>
    public static void Mfm(this List<bool> bits, IEnumerable<byte> bytes, bool previousData = false)
    {
        var previous = previousData;
        foreach (var value in bytes)
        {
            for (var bit = BitPrimitives.BitsPerByte - 1; bit >= 0; bit--)
            {
                var data = (value & (1 << bit)) != 0;
                bits.Add(!previous && !data);
                bits.Add(data);
                previous = data;
            }
        }
    }

    /// <summary>Ajoute des octets encodés en FM, cellule d'horloge puis cellule de données.</summary>
    /// <param name="bits">Tampon recevant les cellules.</param>
    /// <param name="bytes">Octets à encoder.</param>
    public static void Fm(this List<bool> bits, IEnumerable<byte> bytes)
    {
        foreach (var value in bytes)
        {
            for (var bit = BitPrimitives.BitsPerByte - 1; bit >= 0; bit--)
            {
                bits.Add(true);
                bits.Add((value & (1 << bit)) != 0);
            }
        }
    }

    /// <summary>Ajoute des octets en doublant chaque cellule FM.</summary>
    /// <param name="bits">Tampon recevant les cellules.</param>
    /// <param name="bytes">Octets à encoder.</param>
    public static void DoubleFm(this List<bool> bits, IEnumerable<byte> bytes)
    {
        foreach (var value in bytes)
        {
            for (var bit = BitPrimitives.BitsPerByte - 1; bit >= 0; bit--)
            {
                bits.Add(false);
                bits.Add(true);
                bits.Add(false);
                bits.Add((value & (1 << bit)) != 0);
            }
        }
    }

    /// <summary>Ajoute un gap alterné ou entièrement composé de cellules à un.</summary>
    /// <param name="bits">Tampon recevant les cellules.</param>
    /// <param name="count">Nombre de cellules à ajouter.</param>
    /// <param name="allOnes">Indique si toutes les cellules valent un ; sinon le motif commence à un puis alterne.</param>
    /// <exception cref="ArgumentOutOfRangeException">La longueur du gap est négative.</exception>
    public static void Gap(this List<bool> bits, int count, bool allOnes = false)
    {
        if (count < 0) throw TrackEncodingExceptions.NegativeGapLength(count);
        for (var index = 0; index < count; index++) bits.Add(allOnes || (index & BitPrimitives.LeastSignificantBitMask) == 0);
    }

    /// <summary>Encode des octets en MFM puis compacte les cellules obtenues.</summary>
    /// <param name="data">Octets à encoder.</param>
    /// <returns>Cellules MFM compactées, bit de poids fort en premier.</returns>
    public static byte[] EncodeCompactMfm(params byte[] data)
    {
        var bits = new List<bool>(data.Length * EncodedCellCountPerByte);
        bits.Mfm(data);
        return Pack(bits);
    }

    /// <summary>Encode des octets en FM puis compacte les cellules obtenues.</summary>
    /// <param name="data">Octets à encoder.</param>
    /// <returns>Cellules FM compactées, bit de poids fort en premier.</returns>
    public static byte[] EncodeCompactFm(params byte[] data)
    {
        var bits = new List<bool>(data.Length * EncodedCellCountPerByte);
        bits.Fm(data);
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
