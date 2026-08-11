namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Définit la signature et le terminateur de commentaire ImageDisk.</summary>
internal static class ImdFormat
{
    /// <summary>Signature binaire ASCII <c>IMD</c>.</summary>
    public static ReadOnlySpan<byte> Signature => "IMD"u8;
    /// <summary>Longueur de la signature, en octets.</summary>
    public const int SignatureLength = 3;
    /// <summary>Octet terminant le commentaire d'en-tête.</summary>
    public const byte CommentTerminator = 0x1A;
}
