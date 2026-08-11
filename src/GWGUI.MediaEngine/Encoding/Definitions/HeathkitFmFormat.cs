namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Heathkit Fm.</summary>
internal static class HeathkitFmFormat
{
    /// <summary>Définit adresse marque utilisé par ce format.</summary>
    public const byte AddressMark = 0xbf;
    /// <summary>Définit synchronisation zéro nombre utilisé par ce format.</summary>
    public const int SyncZeroCount = 3;
    /// <summary>Définit en-tête octet nombre utilisé par ce format.</summary>
    public const int HeaderByteCount = 4;
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 256;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 160;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Expose secteur marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(FluxEncoding.EncodeFm(0,0,0,AddressMark));
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Heathkit sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
