namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Amiga Mfm.</summary>
internal static class AmigaMfmFormat
{
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Amiga sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    /// <summary>Crée l'exception signalant impair encodé octet nombre.</summary>
    /// <param name="actualCount">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException OddEncodedByteCount(int actualCount) => new($"Amiga odd/even encoding requires an even byte count; received {actualCount} bytes.");
    /// <summary>Définit synchronisation word utilisé par ce format.</summary>
    public const ushort SyncWord = 0x4489;
    /// <summary>Définit synchronisation word nombre utilisé par ce format.</summary>
    public const int SyncWordCount = 2;
    /// <summary>Définit encodé octet bit nombre utilisé par ce format.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Définit synchronisation bit nombre utilisé par ce format.</summary>
    public const int SyncBitCount = SyncWordCount * EncodedByteBitCount;
    /// <summary>Définit encodé secteur octet nombre utilisé par ce format.</summary>
    public const int EncodedSectorByteCount = 540;
    /// <summary>Définit encodé en-tête octet nombre utilisé par ce format.</summary>
    public const int EncodedHeaderByteCount = 28;
    /// <summary>Définit encodé données offset utilisé par ce format.</summary>
    public const int EncodedDataOffset = EncodedHeaderByteCount;
    /// <summary>Définit secteur octet nombre utilisé par ce format.</summary>
    public const int SectorByteCount = 512;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Définit format octet utilisé par ce format.</summary>
    public const byte FormatByte = 0xff;
    /// <summary>Définit info octet nombre utilisé par ce format.</summary>
    public const int InfoByteCount = 4;
    /// <summary>Définit label octet nombre utilisé par ce format.</summary>
    public const int LabelByteCount = 16;
    /// <summary>Définit en-tête parity source octet nombre utilisé par ce format.</summary>
    public const int HeaderParitySourceByteCount = 20;
    /// <summary>Définit en-tête parity haut offset utilisé par ce format.</summary>
    public const int HeaderParityHighOffset = 22;
    /// <summary>Définit en-tête parity bas offset utilisé par ce format.</summary>
    public const int HeaderParityLowOffset = 23;
    /// <summary>Définit données parity haut offset utilisé par ce format.</summary>
    public const int DataParityHighOffset = 26;
    /// <summary>Définit données parity bas offset utilisé par ce format.</summary>
    public const int DataParityLowOffset = 27;
    /// <summary>Définit parity field octet nombre utilisé par ce format.</summary>
    public const int ParityFieldByteCount = 8;
    /// <summary>Définit leading intervalle bit nombre utilisé par ce format.</summary>
    public const int LeadingGapBitCount = 100;
    /// <summary>Définit trailing intervalle bit nombre utilisé par ce format.</summary>
    public const int TrailingGapBitCount = 128;
    /// <summary>Définit nibble bit nombre utilisé par ce format.</summary>
    public const int NibbleBitCount = 4;
}
