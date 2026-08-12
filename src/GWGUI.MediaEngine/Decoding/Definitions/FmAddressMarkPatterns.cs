namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les motifs physiques FM communs aux formats utilisant les marques d'adresse IBM.</summary>
internal static class FmAddressMarkPatterns
{
    /// <summary>Retourne le motif physique correspondant à la marque décodée.</summary>
    public static IReadOnlyList<byte> For(byte mark) => mark switch
    {
        0xf8 => Pattern("55111444"),
        0xf9 => Pattern("55111445"),
        0xfa => Pattern("55111454"),
        0xfb => Pattern("55111455"),
        0xfc => Pattern("55111544"),
        0xfd => Pattern("55111545"),
        0xfe => Pattern("55111554"),
        _ => throw new ArgumentOutOfRangeException(nameof(mark), mark, "Unsupported FM address mark.")
    };

    /// <summary>Convertit le motif hexadécimal en octets physiques immuables.</summary>
    private static IReadOnlyList<byte> Pattern(string hexadecimal) => Array.AsReadOnly(Convert.FromHexString(hexadecimal));
}
