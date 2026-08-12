namespace GWGUI.MediaEngine.Primitives;

/// <summary>Calcule la somme de contrôle XOR avec rotation utilisée par Heathkit et NorthStar.</summary>
internal static class RotatingChecksumCalculator
{
    /// <summary>Nombre de positions de la rotation vers la gauche.</summary>
    private const int LeftRotation = 1;
    /// <summary>Nombre de positions du complément de rotation dans un octet.</summary>
    private const int RightRotation = BitPrimitives.BitsPerByte - LeftRotation;

    /// <summary>Calcule le checksum en appliquant un XOR puis une rotation d'un bit après chaque octet.</summary>
    /// <param name="values">Octets à traiter dans leur ordre logique.</param>
    /// <returns>Checksum rotatif sur huit bits.</returns>
    public static byte Compute(IEnumerable<byte> values)
    {
        byte checksum = 0;
        foreach (var value in values)
        {
            checksum ^= value;
            checksum = (byte)((checksum >> RightRotation) | (checksum << LeftRotation));
        }
        return checksum;
    }
}
