namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Data General Fm.</summary>
internal static class DataGeneralFmFormat
{
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 512;
    /// <summary>Définit somme de contrôle octet nombre utilisé par ce format.</summary>
    public const int ChecksumByteCount = 2;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 64;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Définit cylindre masque utilisé par ce format.</summary>
    public const byte CylinderMask = 0x7f;
    /// <summary>Définit face masque utilisé par ce format.</summary>
    public const byte HeadMask = 0x80;
    /// <summary>Définit face décalage utilisé par ce format.</summary>
    public const int HeadShift = 7;
    /// <summary>Définit secteur décalage utilisé par ce format.</summary>
    public const int SectorShift = 2;
    /// <summary>Expose synchronisation utilisé par ce format.</summary>
    public static IReadOnlyList<byte> Sync { get; } = Array.AsReadOnly(FluxEncoding.EncodeFm(0x00,0x01));
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Data General sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
