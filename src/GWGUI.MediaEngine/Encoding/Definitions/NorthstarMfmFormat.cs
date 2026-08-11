namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Northstar Mfm.</summary>
internal static class NorthstarMfmFormat
{
    /// <summary>Définit adresse marque utilisé par ce format.</summary>
    public const byte AddressMark = 0xfb;
    /// <summary>Définit synchronisation zéro nombre utilisé par ce format.</summary>
    public const int SyncZeroCount = 7;
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 512;
    /// <summary>Définit cylindre décalage utilisé par ce format.</summary>
    public const int CylinderShift = 4;
    /// <summary>Définit secteur masque utilisé par ce format.</summary>
    public const byte SectorMask = 0x0f;
    /// <summary>Définit intervalle bit nombre utilisé par ce format.</summary>
    public const int GapBitCount = 128;
    /// <summary>Expose secteur marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(FluxEncoding.EncodeMfm(0,0,0,0,0,0,0,AddressMark));
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"NorthStar sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
