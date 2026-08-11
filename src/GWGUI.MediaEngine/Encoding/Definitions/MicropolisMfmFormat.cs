namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Micropolis Mfm.</summary>
internal static class MicropolisMfmFormat
{
    /// <summary>Définit adresse marque utilisé par ce format.</summary>
    public const byte AddressMark = 0xff;
    /// <summary>Définit synchronisation zéro nombre utilisé par ce format.</summary>
    public const int SyncZeroCount = 3;
    /// <summary>Définit enregistrement identité octet nombre utilisé par ce format.</summary>
    public const int RecordIdentityByteCount = 3;
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 256;
    /// <summary>Définit en-tête remplissage octet nombre utilisé par ce format.</summary>
    public const int HeaderPaddingByteCount = 10;
    /// <summary>Définit fin remplissage octet nombre utilisé par ce format.</summary>
    public const int TrailerPaddingByteCount = 5;
    /// <summary>Définit enregistrement octet nombre utilisé par ce format.</summary>
    public const int RecordByteCount = 275;
    /// <summary>Définit préambule octet nombre utilisé par ce format.</summary>
    public const int PreambleByteCount = 40;
    /// <summary>Définit intervalle bit nombre utilisé par ce format.</summary>
    public const int GapBitCount = 128;
    /// <summary>Définit somme de contrôle modulus utilisé par ce format.</summary>
    public const int ChecksumModulus = 255;
    /// <summary>Expose synchronisation utilisé par ce format.</summary>
    public static IReadOnlyList<byte> Sync { get; } = Array.AsReadOnly(FluxEncoding.EncodeMfm(0,0,0,AddressMark));
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Micropolis sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
