namespace GWGUI.Scp.Containers.Scp;

/// <summary>
/// Regroupe les dimensions fixes de l'en-tête et de la table des pistes du format de conteneur SCP.
/// </summary>
public static class ScpFormatConstants
{
    /// <summary>
    /// Taille de l'en-tête SCP, en octets.
    /// </summary>
    public const int HeaderLength = 16;

    /// <summary>
    /// Nombre maximal d'entrées de piste ou de face adressables dans la table SCP.
    /// </summary>
    public const int FloppyTrackSlots = 168;

    /// <summary>
    /// Position, en octets depuis le début du fichier, de la table des pistes SCP.
    /// </summary>
    public const int TrackTableOffset = 0x10;
}
