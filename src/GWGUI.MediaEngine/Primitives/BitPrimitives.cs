namespace GWGUI.MediaEngine.Primitives;

/// <summary>Fournit les constantes et opérations élémentaires communes aux traitements de bits.</summary>
internal static class BitPrimitives
{
    /// <summary>Nombre de bits contenus dans un octet.</summary>
    public const int BitsPerByte = 8;
    /// <summary>Masque sélectionnant le bit de poids faible d'une valeur.</summary>
    public const int LeastSignificantBitMask = 1;
    /// <summary>Inverse l'ordre des bits d'un octet.</summary>
    /// <param name="value">Octet dont les bits doivent être inversés.</param>
    /// <returns>Octet obtenu après inversion de l'ordre des bits.</returns>
    public static byte ReverseBits(byte value)
    {
        var result = 0;
        for (var bit = 0; bit < BitsPerByte; bit++) result = result << LeastSignificantBitMask | value >> bit & LeastSignificantBitMask;
        return (byte)result;
    }
}
