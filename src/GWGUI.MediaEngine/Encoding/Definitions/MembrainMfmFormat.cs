namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Membrain Mfm.</summary>
internal static class MembrainMfmFormat
{
    /// <summary>Définit synchronisation octet utilisé par ce format.</summary>
    public const byte SyncByte = 0xa1;
    /// <summary>Définit en-tête adresse marque utilisé par ce format.</summary>
    public const byte HeaderAddressMark = 0xfe;
    /// <summary>Définit données adresse marque utilisé par ce format.</summary>
    public const byte DataAddressMark = 0xf8;
    /// <summary>Définit last données adresse marque utilisé par ce format.</summary>
    public const byte LastDataAddressMark = 0xfb;
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 512;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit cylindre bas bit nombre utilisé par ce format.</summary>
    public const int CylinderLowBitCount = 3;
    /// <summary>Définit cylindre bas décalage utilisé par ce format.</summary>
    public const int CylinderLowShift = 5;
    /// <summary>Définit face décalage utilisé par ce format.</summary>
    public const int HeadShift = 4;
    /// <summary>Définit cylindre haut masque utilisé par ce format.</summary>
    public const byte CylinderHighMask = 0x1f;
    /// <summary>Définit cylindre bas valeur masque utilisé par ce format.</summary>
    public const byte CylinderLowValueMask = 0x07;
    /// <summary>Définit cylindre bas masque utilisé par ce format.</summary>
    public const byte CylinderLowMask = 0xe0;
    /// <summary>Définit face masque utilisé par ce format.</summary>
    public const byte HeadMask = 1;
    /// <summary>Définit secteur masque utilisé par ce format.</summary>
    public const byte SectorMask = 0x0f;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 64;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Définit crc polynôme utilisé par ce format.</summary>
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.IbmPolynomial;
    /// <summary>Définit crc initiale valeur utilisé par ce format.</summary>
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.ZeroInitialValue;
    /// <summary>Expose secteur en-tête utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorHeader { get; } = Array.AsReadOnly<byte>([0x44,0x89,0x55,0x54]);
    /// <summary>Expose secteur données utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorData { get; } = Array.AsReadOnly<byte>([0x44,0x89,0x55,0x4a]);
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Membrain sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
