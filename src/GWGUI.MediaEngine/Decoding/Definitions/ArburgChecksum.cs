namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Calcule et contrôle le checksum additif 16 bits des blocs Arburg.</summary>
internal static class ArburgChecksum
{
    /// <summary>Calcule la somme non signée des octets utiles.</summary>
    /// <param name="values">Octets utiles couverts par le checksum.</param>
    /// <returns>Somme réduite à 16 bits.</returns>
    public static ushort Calculate(IEnumerable<byte> values)
    {
        ushort checksum = 0;
        foreach (var value in values) checksum += value;
        return checksum;
    }

    /// <summary>Contrôle les deux octets de checksum placés après les données utiles.</summary>
    /// <param name="block">Bloc physique décodé.</param>
    /// <param name="usefulSize">Nombre d'octets utiles couverts.</param>
    /// <returns><see langword="true"/> lorsque les deux octets correspondent à la somme calculée.</returns>
    public static bool IsValid(IReadOnlyList<byte> block, int usefulSize)
    {
        if (block.Count < usefulSize + ArburgFormat.ChecksumByteCount) return false;
        var checksum = Calculate(block.Take(usefulSize));
        return block[usefulSize] == (byte)checksum && block[usefulSize + 1] == (byte)(checksum >> Primitives.BitPrimitives.BitsPerByte);
    }

    /// <summary>Construit un bloc physique contenant les données utiles, leur checksum et le remplissage nul.</summary>
    /// <param name="data">Données utiles.</param>
    /// <param name="totalSize">Taille physique du bloc à produire.</param>
    /// <returns>Bloc physique complet.</returns>
    public static byte[] CreateBlock(IReadOnlyList<byte> data, int totalSize)
    {
        var block = new byte[totalSize];
        for (var index = 0; index < data.Count; index++) block[index] = data[index];
        var checksum = Calculate(data);
        block[data.Count] = (byte)checksum;
        block[data.Count + 1] = (byte)(checksum >> Primitives.BitPrimitives.BitsPerByte);
        return block;
    }
}
