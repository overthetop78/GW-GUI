namespace GWGUI.MediaEngine.Containers.Cp2;

/// <summary>Définit la signature du conteneur SNATCH-IT CP2.</summary>
internal static class Cp2Format
{
    /// <summary>Signature binaire <c>SOFTWARE PIRATES</c>.</summary>
    public static ReadOnlySpan<byte> Signature => "SOFTWARE PIRATES"u8;
    /// <summary>Longueur de la signature CP2.</summary>
    public const int SignatureLength = 16;
}
