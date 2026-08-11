namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Arburg.</summary>
internal static class ArburgFormat
{
    /// <summary>Définit system attribut utilisé par ce format.</summary>
    public const string SystemAttribute = "system";
    /// <summary>Définit données bloc taille utilisé par ce format.</summary>
    public const int DataBlockSize = 0xa00;
    /// <summary>Définit données useful taille utilisé par ce format.</summary>
    public const int DataUsefulSize = 0x9fe;
    /// <summary>Définit system bloc taille utilisé par ce format.</summary>
    public const int SystemBlockSize = 0xf00;
    /// <summary>Définit system useful taille utilisé par ce format.</summary>
    public const int SystemUsefulSize = 0xefe;
    /// <summary>Définit somme de contrôle octet nombre utilisé par ce format.</summary>
    public const int ChecksumByteCount = 2;
    /// <summary>Définit intervalle bit nombre utilisé par ce format.</summary>
    public const int GapBitCount = 64;
    /// <summary>Expose données marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0x44,0x44,0x44,0x44,0x55,0x55,0x55,0x55]);
    /// <summary>Expose system marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SystemMark { get; } = Array.AsReadOnly<byte>([0x55,0x55,0x55,0x55,0x55,0x24,0x92,0x49]);
    /// <summary>Crée l'exception signalant invalide charge utile taille.</summary>
    /// <param name="system">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidPayloadSize(bool system, int actualSize) => new($"Arburg {(system ? "system" : "data")} payload must contain {(system ? SystemUsefulSize : DataUsefulSize)} useful bytes or {(system ? SystemBlockSize : DataBlockSize)} complete bytes; received {actualSize} bytes.");
}
