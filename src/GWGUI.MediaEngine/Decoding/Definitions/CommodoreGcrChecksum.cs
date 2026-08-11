namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Calcule le checksum XOR commun aux enregistrements GCR Commodore.</summary>
internal static class CommodoreGcrChecksum
{
    /// <summary>Calcule le XOR de tous les octets fournis.</summary>
    /// <param name="values">Octets à inclure.</param><returns>Checksum XOR.</returns>
    public static byte Calculate(IEnumerable<byte> values)
    {
        byte checksum = 0;
        foreach (var value in values) checksum ^= value;
        return checksum;
    }

    /// <summary>Indique si un enregistrement incluant son checksum produit un XOR nul.</summary>
    /// <param name="values">Enregistrement complet.</param><returns><see langword="true"/> si son XOR est nul.</returns>
    public static bool IsValid(IEnumerable<byte> values) => Calculate(values) == 0;
}
