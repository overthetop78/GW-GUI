namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Définit les signatures du conteneur TeleDisk.</summary>
internal static class Td0Format
{
    /// <summary>Version TeleDisk émise par le Writer.</summary>
    public const byte DefaultVersion = 21;
    /// <summary>Débit 250 kbit/s utilisé par défaut.</summary>
    public const byte DefaultDataRate = 0;
    /// <summary>Signature d'un conteneur non compressé.</summary>
    public static ReadOnlySpan<byte> UncompressedSignature => "TD"u8;
    /// <summary>Signature d'un conteneur utilisant la compression avancée.</summary>
    public static ReadOnlySpan<byte> AdvancedCompressionSignature => "td"u8;
    /// <summary>Longueur d'une signature, en octets.</summary>
    public const int SignatureLength = 2;

}
