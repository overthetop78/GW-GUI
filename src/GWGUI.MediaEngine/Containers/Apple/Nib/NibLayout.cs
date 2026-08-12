namespace GWGUI.MediaEngine.Containers.Apple.Nib;

/// <summary>Décrit la longueur fixe des pistes d'un conteneur NIB Apple II.</summary>
internal static class NibLayout
{
    /// <summary>Longueur exacte d'une piste NIB, soit 6 656 octets.</summary>
    public const int TrackLengthBytes = 6656;
    /// <summary>Octet remplissant les bits inutilisés d'une piste NIB.</summary>
    public const byte TrackFillByte = 0xff;
    /// <summary>Nombre maximal de bits stockables dans une piste NIB.</summary>
    public const int MaximumTrackBitCount = TrackLengthBytes * Primitives.BitPrimitives.BitsPerByte;
}
