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

    /// <summary>Taille de l'en-tête SCP, en octets.</summary>
    public const int HeaderLength = ChecksumOffset + ChecksumLength;

    /// <summary>
    /// Nombre maximal d'entrées de piste ou de face adressables dans la table SCP.
    /// </summary>
    public const int FloppyTrackSlots = 168;

    /// <summary>
    /// Position, en octets depuis le début du fichier, de la table des pistes SCP.
    /// </summary>
    public const int TrackTableOffset = HeaderLength;

    /// <summary>Position de la version dans l’en-tête SCP.</summary>
    public const int VersionOffset = SignatureLength;

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
    public const int TrackTableEntrySize = sizeof(uint);

    /// <summary>Longueur fixe, en octets, de l’en-tête d’un descripteur de piste.</summary>
    public const int TrackDescriptorHeaderSize = 4;

    /// <summary>Longueur, en octets, d’un descripteur de révolution.</summary>
    public const int RevolutionDescriptorSize = RevolutionDataOffset + sizeof(uint);

    /// <summary>Position du numéro de piste dans le descripteur de piste.</summary>
    public const int TrackNumberOffset = SignatureLength;

    /// <summary>Position du temps d’index dans un descripteur de révolution.</summary>
    public const int RevolutionIndexTimeOffset = 0;

    /// <summary>Position du nombre de flux dans un descripteur de révolution.</summary>
    public const int RevolutionFluxCountOffset = RevolutionIndexTimeOffset + sizeof(uint);

    /// <summary>Position de l’offset relatif des flux dans un descripteur de révolution.</summary>
    public const int RevolutionDataOffset = RevolutionFluxCountOffset + sizeof(uint);

    /// <summary>Longueur, en octets, d’un intervalle de flux encodé dans un fichier SCP.</summary>
    public const int FluxIntervalSize = sizeof(ushort);

    /// <summary>Valeur ajoutée lorsqu’un intervalle de flux encodé vaut zéro.</summary>
    public const uint ZeroFluxIntervalOverflow = (uint)ushort.MaxValue + 1u;

    /// <summary>Nombre minimal de révolutions accepté dans un en-tête SCP.</summary>
    public const byte MinimumRevolutionCount = 1;

    /// <summary>Nombre maximal de révolutions accepté dans un en-tête SCP.</summary>
    public const byte MaximumRevolutionCount = 64;

    /// <summary>Durée élémentaire, en nanosecondes, d’un pas de résolution SCP.</summary>
    public const int ResolutionStepNanoseconds = 25;

    /// <summary>Décalage ajouté à l’indice de résolution stocké dans l’en-tête SCP.</summary>
    public const int ResolutionIndexOffset = 1;

    /// <summary>Nombre de bits séparant les composantes majeure et mineure de la version SCP.</summary>
    public const int VersionMajorShift = 4;

    /// <summary>Masque permettant d’extraire la composante mineure de la version SCP.</summary>
    public const byte VersionMinorMask = 0x0f;

    /// <summary>Valeur initiale d’un calcul de somme de contrôle SCP.</summary>
    public const uint InitialChecksum = 0u;

    /// <summary>Valeur déclarée indiquant l’absence de somme de contrôle exploitable pour une capture réinscriptible.</summary>
    public const uint MissingChecksum = 0u;
}
