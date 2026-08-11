namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Hp Mmfm.</summary>
internal static class HpMmfmFormat
{
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 256;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit encodé données octet nombre utilisé par ce format.</summary>
    public const int EncodedDataByteCount = SectorSize + CrcByteCount;
    /// <summary>Définit face décalage utilisé par ce format.</summary>
    public const int HeadShift = 7;
    /// <summary>Définit secteur masque utilisé par ce format.</summary>
    public const byte SectorMask = 0x7f;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 128;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 256;
    /// <summary>Expose secteur synchronisation utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorSync { get; } = Array.AsReadOnly<byte>([0x55,0x55,0x2a,0x54]);
    /// <summary>Expose données synchronisation utilisé par ce format.</summary>
    public static IReadOnlyList<byte> DataSync { get; } = Array.AsReadOnly<byte>([0x55,0x55,0x2a,0x44]);
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"HP MMFM sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
