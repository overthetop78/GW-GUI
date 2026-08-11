namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Tycom Fm.</summary>
internal static class TycomFmFormat
{
    /// <summary>Définit en-tête adresse marque utilisé par ce format.</summary>
    public const byte HeaderAddressMark = 0xfe;
    /// <summary>Définit supprimées données marque utilisé par ce format.</summary>
    public const byte DeletedDataMark = 0xf8;
    /// <summary>Définit données marque f9 utilisé par ce format.</summary>
    public const byte DataMarkF9 = 0xf9;
    /// <summary>Définit données marque fa utilisé par ce format.</summary>
    public const byte DataMarkFa = 0xfa;
    /// <summary>Définit données marque utilisé par ce format.</summary>
    public const byte DataMark = 0xfb;
    /// <summary>Définit en-tête décodé octet nombre utilisé par ce format.</summary>
    public const int HeaderDecodedByteCount = 4;
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 128;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit intervalle bit nombre utilisé par ce format.</summary>
    public const int GapBitCount = 64;
    /// <summary>Définit données recherche octet nombre utilisé par ce format.</summary>
    public const int DataSearchByteCount = 88 + 16;
    /// <summary>Définit crc polynôme utilisé par ce format.</summary>
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.CcittPolynomial;
    /// <summary>Définit crc initiale valeur utilisé par ce format.</summary>
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.AllBitsSetInitialValue;
    /// <summary>Expose en-tête marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly<byte>([0x55,0x11,0x15,0x54]);
    /// <summary>Expose données marks utilisé par ce format.</summary>
    public static IReadOnlyList<(IReadOnlyList<byte> Pattern, byte Mark)> DataMarks { get; } = Array.AsReadOnly<(IReadOnlyList<byte>,byte)>([(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x44]),DeletedDataMark),(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x45]),DataMarkF9),(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x54]),DataMarkFa),(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x55]),DataMark)]);
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"TYCOM sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
