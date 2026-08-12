namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les motifs physiques FM communs aux formats utilisant les marques d'adresse IBM.</summary>
internal static class FmAddressMarkPatterns
{
    /// <summary>Motif FM doublé de la marque de données supprimées <c>0xF8</c>.</summary>
    private static IReadOnlyList<byte> DeletedData { get; } = Pattern("55111444");
    /// <summary>Motif FM doublé de la marque <c>0xF9</c>.</summary>
    private static IReadOnlyList<byte> AlternateData { get; } = Pattern("55111445");
    /// <summary>Motif FM doublé de la marque <c>0xFA</c>.</summary>
    private static IReadOnlyList<byte> AlternateDeletedData { get; } = Pattern("55111454");
    /// <summary>Motif FM doublé de la marque de données <c>0xFB</c>.</summary>
    private static IReadOnlyList<byte> Data { get; } = Pattern("55111455");
    /// <summary>Motif FM doublé de la marque d'index <c>0xFC</c>.</summary>
    private static IReadOnlyList<byte> Index { get; } = Pattern("55111544");
    /// <summary>Motif FM doublé de la marque <c>0xFD</c>.</summary>
    private static IReadOnlyList<byte> AlternateIndex { get; } = Pattern("55111545");
    /// <summary>Motif FM doublé de la marque d'identification <c>0xFE</c>.</summary>
    private static IReadOnlyList<byte> Identifier { get; } = Pattern("55111554");

    /// <summary>Retourne le motif physique correspondant à la marque décodée.</summary>
    /// <param name="mark">Octet de marque FM décodé.</param>
    /// <returns>Octets contenant les cellules d'horloge et de données du motif.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La marque ne possède aucune définition commune.</exception>
    public static IReadOnlyList<byte> For(byte mark) => mark switch
    {
        0xf8 => DeletedData,
        0xf9 => AlternateData,
        0xfa => AlternateDeletedData,
        0xfb => Data,
        0xfc => Index,
        0xfd => AlternateIndex,
        0xfe => Identifier,
        _ => throw new ArgumentOutOfRangeException(nameof(mark), mark, "Unsupported FM address mark.")
    };

    /// <summary>Convertit le motif hexadécimal en octets physiques immuables.</summary>
    private static IReadOnlyList<byte> Pattern(string hexadecimal) => Array.AsReadOnly(Convert.FromHexString(hexadecimal));
}
