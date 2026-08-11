namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Aed6200p Mfm.</summary>
internal static class Aed6200pMfmFormat
{
    /// <summary>Définit en-tête adresse marque utilisé par ce format.</summary>
    public const byte HeaderAddressMark = 0xc6;
    /// <summary>Définit supprimées données marque utilisé par ce format.</summary>
    public const byte DeletedDataMark = 0xc0;
    /// <summary>Définit données marque utilisé par ce format.</summary>
    public const byte DataMark = 0xc3;
    /// <summary>Définit en-tête octet nombre utilisé par ce format.</summary>
    public const int HeaderByteCount = 7;
    /// <summary>Définit données marque octet nombre utilisé par ce format.</summary>
    public const int DataMarkByteCount = 1;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit premier intervalle bit nombre utilisé par ce format.</summary>
    public const int FirstGapBitCount = 64;
    /// <summary>Définit second intervalle bit nombre utilisé par ce format.</summary>
    public const int SecondGapBitCount = 128;
    /// <summary>Expose en-tête motif utilisé par ce format.</summary>
    public static IReadOnlyList<byte> HeaderPattern { get; } = Array.AsReadOnly<byte>([0x50,0x94]);
    /// <summary>Expose données motifs utilisé par ce format.</summary>
    public static IReadOnlyList<IReadOnlyList<byte>> DataPatterns { get; } = Array.AsReadOnly<IReadOnlyList<byte>>([Array.AsReadOnly<byte>([0x50,0x8a]),Array.AsReadOnly<byte>([0x50,0x89]),Array.AsReadOnly<byte>([0x50,0x84]),Array.AsReadOnly<byte>([0x50,0x85])]);
}
