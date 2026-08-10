namespace GWGUI.MediaEngine.Recognition.Apple;

/// <summary>Regroupe les dimensions fixes d’une piste NIB Apple II.</summary>
internal static class NibTrackFormat
{
    /// <summary>Longueur exacte, en octets, d’une piste NIB.</summary>
    public const int TrackLength = 6656;
    /// <summary>Nombre de bits représentés par un octet de piste NIB.</summary>
    public const int BitsPerByte = 8;
}
