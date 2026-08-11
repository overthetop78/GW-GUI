namespace GWGUI.MediaEngine.Primitives;

/// <summary>Calcule des CRC 16 bits non réfléchis à partir d'un polynôme et d'une valeur initiale configurables.</summary>
internal static class Crc16Calculator
{
    /// <summary>Polynôme <c>0x1021</c> utilisé par la variante CRC-16/CCITT.</summary>
    public const ushort CcittPolynomial = 0x1021;
    /// <summary>Polynôme <c>0x8005</c> utilisé par la variante CRC-16/IBM non réfléchie.</summary>
    public const ushort IbmPolynomial = 0x8005;
    /// <summary>Valeur initiale <c>0xFFFF</c> utilisée par défaut avec la variante CCITT.</summary>
    public const ushort AllBitsSetInitialValue = 0xFFFF;
    /// <summary>Valeur initiale nulle utilisée notamment par les variantes IBM et CCITT qui l'exigent.</summary>
    public const ushort ZeroInitialValue = 0x0000;
    /// <summary>Masque du bit de poids fort du registre CRC 16 bits.</summary>
    public const ushort HighBitMask = 0x8000;
    /// <summary>Nombre de positions nécessaires pour placer un octet dans la partie haute du registre CRC.</summary>
    public const int ByteShift = BitPrimitives.BitsPerByte;

    /// <summary>Calcule le CRC d'une séquence complète d'octets.</summary>
    /// <param name="values">Octets traités dans leur ordre d'énumération.</param>
    /// <param name="polynomial">Polynôme non réfléchi ; utilise <see cref="CcittPolynomial"/> par défaut.</param>
    /// <param name="initial">Valeur initiale du registre ; utilise <see cref="AllBitsSetInitialValue"/> par défaut.</param>
    /// <returns>Valeur finale du registre CRC 16 bits.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> est nul.</exception>
    public static ushort Compute(IEnumerable<byte> values, ushort polynomial = CcittPolynomial, ushort initial = AllBitsSetInitialValue)
    {
        ArgumentNullException.ThrowIfNull(values);
        var crc = initial;
        foreach (var value in values) crc = Update(crc, value, polynomial);
        return crc;
    }

    /// <summary>Met à jour un registre CRC avec un octet supplémentaire.</summary>
    /// <param name="crc">Valeur courante du registre CRC.</param>
    /// <param name="value">Octet à intégrer au registre.</param>
    /// <param name="polynomial">Polynôme non réfléchi ; utilise <see cref="CcittPolynomial"/> par défaut.</param>
    /// <returns>Nouvelle valeur du registre CRC 16 bits.</returns>
    public static ushort Update(ushort crc, byte value, ushort polynomial = CcittPolynomial)
    {
        crc ^= (ushort)(value << ByteShift);
        for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++) crc = (ushort)((crc & HighBitMask) != 0 ? (crc << 1) ^ polynomial : crc << 1);
        return crc;
    }
}
