namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Micral NFm.</summary>
internal static class MicralNFmFormat
{
    /// <summary>Définit adresse marque utilisé par ce format.</summary>
    public const byte AddressMark = 0xff;
    /// <summary>Définit synchronisation zéro nombre utilisé par ce format.</summary>
    public const int SyncZeroCount = 3;
    /// <summary>Définit identité octet nombre utilisé par ce format.</summary>
    public const int IdentityByteCount = 2;
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 128;
    /// <summary>Définit somme de contrôle octet nombre utilisé par ce format.</summary>
    public const int ChecksumByteCount = 1;
    /// <summary>Définit carry masque utilisé par ce format.</summary>
    public const byte CarryMask = 0x80;
    /// <summary>Définit complement masque utilisé par ce format.</summary>
    public const byte ComplementMask = 0xff;
    /// <summary>Définit intervalle bit nombre utilisé par ce format.</summary>
    public const int GapBitCount = 128;
    /// <summary>Expose secteur marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(FluxEncoding.EncodeFm(0,0,0,AddressMark));
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Micral N sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
