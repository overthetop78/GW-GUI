namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Emu Fm.</summary>
internal static class EmuFmFormat
{
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 0xe00;
    /// <summary>Définit en-tête décodé octet nombre utilisé par ce format.</summary>
    public const int HeaderDecodedByteCount = 3;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit piste décalage utilisé par ce format.</summary>
    public const int TrackShift = 1;
    /// <summary>Définit face masque utilisé par ce format.</summary>
    public const byte HeadMask = 1;
    /// <summary>Définit intervalle bit nombre utilisé par ce format.</summary>
    public const int GapBitCount = 64;
    /// <summary>Définit crc polynôme utilisé par ce format.</summary>
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.IbmPolynomial;
    /// <summary>Définit crc initiale valeur utilisé par ce format.</summary>
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.ZeroInitialValue;
    /// <summary>Expose secteur marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly<byte>([0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45]);
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"E-mu sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
