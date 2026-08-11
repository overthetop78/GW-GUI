namespace GWGUI.MediaEngine.Primitives;

/// <summary>Fournit les constantes et opérations élémentaires communes aux traitements de bits.</summary>
internal static class BitPrimitives
{
    /// <summary>Nombre de bits contenus dans un octet.</summary>
    public const int BitsPerByte = 8;
    /// <summary>Inverse l'ordre des bits d'un octet.</summary>
    /// <param name="value">Octet dont les bits doivent être inversés.</param>
    /// <returns>Octet obtenu après inversion de l'ordre des bits.</returns>
    public static byte Reverse(byte value)
    {
        var result = 0;
        for (var bit = 0; bit < BitsPerByte; bit++) result = result << 1 | value >> bit & 1;
        return (byte)result;
    }
}
