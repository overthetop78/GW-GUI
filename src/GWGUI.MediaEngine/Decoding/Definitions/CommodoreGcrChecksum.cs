namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Calcule le checksum XOR commun aux enregistrements GCR Commodore.</summary>
internal static class CommodoreGcrChecksum
{
    /// <summary>Calcule le XOR de tous les octets fournis.</summary>
    public static byte Calculate(IEnumerable<byte> values)
    {
        byte checksum = 0;
        foreach (var value in values) checksum ^= value;
        return checksum;
    }

    /// <summary>Indique si un enregistrement incluant son checksum produit un XOR nul.</summary>
    public static bool IsValid(IEnumerable<byte> values) => Calculate(values) == 0;
}
