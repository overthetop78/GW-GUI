namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Regroupe les dimensions fixes de l'en-tête et de la table des pistes du format de conteneur SCP.
/// </summary>
public static class ScpFormatConstants
{
    /// <summary>Signature ASCII placée au début d’un fichier SCP.</summary>
    public static ReadOnlySpan<byte> FileSignature => "SCP"u8;

    /// <summary>Signature ASCII placée au début d’une piste SCP.</summary>
    public static ReadOnlySpan<byte> TrackSignature => "TRK"u8;

    /// <summary>Longueur, en octets, des signatures SCP et TRK.</summary>
    public const int SignatureLength = 3;

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

    /// <summary>Position de la version dans l’en-tête SCP.</summary>
    public const int VersionOffset = 3;

    /// <summary>Position du type de disque dans l’en-tête SCP.</summary>
    public const int DiskTypeOffset = 4;

    /// <summary>Position du nombre de révolutions dans l’en-tête SCP.</summary>
    public const int RevolutionCountOffset = 5;

    /// <summary>Position de la première piste dans l’en-tête SCP.</summary>
    public const int StartTrackOffset = 6;

    /// <summary>Position de la dernière piste dans l’en-tête SCP.</summary>
    public const int EndTrackOffset = 7;

    /// <summary>Position des drapeaux dans l’en-tête SCP.</summary>
    public const int FlagsOffset = 8;

    /// <summary>Position de la largeur de cellule de bit dans l’en-tête SCP.</summary>
    public const int BitCellWidthOffset = 9;

    /// <summary>Position du sélecteur de tête dans l’en-tête SCP.</summary>
    public const int HeadsOffset = 10;

    /// <summary>Position de la résolution temporelle dans l’en-tête SCP.</summary>
    public const int ResolutionOffset = 11;

    /// <summary>Position de la somme de contrôle dans l’en-tête SCP.</summary>
    public const int ChecksumOffset = 12;

    /// <summary>Longueur, en octets, de la somme de contrôle SCP.</summary>
    public const int ChecksumLength = 4;

    /// <summary>Longueur, en octets, d’une entrée de la table des pistes.</summary>
    public const int TrackTableEntrySize = 4;

    /// <summary>Longueur fixe, en octets, de l’en-tête d’un descripteur de piste.</summary>
    public const int TrackDescriptorHeaderSize = 4;

    /// <summary>Longueur, en octets, d’un descripteur de révolution.</summary>
    public const int RevolutionDescriptorSize = 12;

    /// <summary>Position du numéro de piste dans le descripteur de piste.</summary>
    public const int TrackNumberOffset = 3;

    /// <summary>Position du temps d’index dans un descripteur de révolution.</summary>
    public const int RevolutionIndexTimeOffset = 0;

    /// <summary>Position du nombre de flux dans un descripteur de révolution.</summary>
    public const int RevolutionFluxCountOffset = 4;

    /// <summary>Position de l’offset relatif des flux dans un descripteur de révolution.</summary>
    public const int RevolutionDataOffset = 8;

    /// <summary>Longueur, en octets, d’un intervalle de flux encodé dans un fichier SCP.</summary>
    public const int FluxIntervalSize = 2;

    /// <summary>Valeur ajoutée lorsqu’un intervalle de flux encodé vaut zéro.</summary>
    public const uint ZeroFluxIntervalOverflow = 65536;

    /// <summary>Nombre minimal de révolutions accepté dans un en-tête SCP.</summary>
    public const byte MinimumRevolutionCount = 1;

    /// <summary>Nombre maximal de révolutions accepté dans un en-tête SCP.</summary>
    public const byte MaximumRevolutionCount = 64;

    /// <summary>Largeur standard de cellule de bit déclarée par SCP.</summary>
    public const byte StandardBitCellWidth = 0;

    /// <summary>Largeur alternative de cellule de bit prise en charge.</summary>
    public const byte AlternateBitCellWidth = 16;

    /// <summary>Valeur maximale du sélecteur de tête SCP.</summary>
    public const byte MaximumHeadSelector = 2;
}
