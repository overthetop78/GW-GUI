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

    /// <summary>Durée, en nanosecondes, d’une milliseconde.</summary>
    public const double NanosecondsPerMillisecond = 1_000_000d;

    /// <summary>Durée, en millisecondes, d’une minute.</summary>
    public const double MillisecondsPerMinute = 60_000d;

    /// <summary>Durée élémentaire, en nanosecondes, d’un pas de résolution SCP.</summary>
    public const int ResolutionStepNanoseconds = 25;

    /// <summary>Décalage ajouté à l’indice de résolution stocké dans l’en-tête SCP.</summary>
    public const int ResolutionIndexOffset = 1;

    /// <summary>Nombre de bits séparant les composantes majeure et mineure de la version SCP.</summary>
    public const int VersionMajorShift = 4;

    /// <summary>Masque permettant d’extraire la composante mineure de la version SCP.</summary>
    public const byte VersionMinorMask = 0x0f;

    /// <summary>
    /// Ajoute les octets fournis à une somme de contrôle SCP existante avec un cumul non signé sur 32 bits.
    /// </summary>
    /// <param name="checksum">Somme de contrôle calculée avant les octets fournis.</param>
    /// <param name="data">Octets SCP à ajouter au cumul.</param>
    /// <returns>Somme des octets modulo 2<sup>32</sup>.</returns>
    public static uint UpdateChecksum(uint checksum, ReadOnlySpan<byte> data)
    {
        foreach (var value in data) checksum = unchecked(checksum + value);
        return checksum;
    }

    /// <summary>
    /// Calcule la somme de contrôle SCP des octets fournis avec un cumul non signé sur 32 bits.
    /// </summary>
    /// <param name="data">Octets couverts par la somme de contrôle SCP.</param>
    /// <returns>Somme des octets modulo 2<sup>32</sup>.</returns>
    public static uint ComputeChecksum(ReadOnlySpan<byte> data) => UpdateChecksum(0, data);

    /// <summary>
    /// Indique si une somme de contrôle calculée respecte la valeur déclarée dans l'en-tête SCP.
    /// </summary>
    /// <param name="declaredChecksum">Somme de contrôle enregistrée dans l'en-tête SCP.</param>
    /// <param name="flags">Drapeaux de l'en-tête SCP.</param>
    /// <param name="computedChecksum">Somme de contrôle calculée sur les octets couverts.</param>
    /// <returns>
    /// <see langword="true"/> lorsque les deux sommes sont identiques, ou lorsque la somme déclarée est nulle
    /// pour une capture marquée réinscriptible ; sinon <see langword="false"/>.
    /// </returns>
    public static bool IsChecksumValid(uint declaredChecksum, ScpFlags flags, uint computedChecksum) =>
        declaredChecksum == 0 && (flags & ScpFlags.Writable) != 0 || declaredChecksum == computedChecksum;

    /// <summary>
    /// Convertit un numéro d'entrée de piste SCP en cylindre et face physiques.
    /// </summary>
    /// <param name="trackNumber">Numéro compris entre zéro et <see cref="FloppyTrackSlots"/> moins un.</param>
    /// <returns>Couple formé du cylindre et de la face correspondant au numéro SCP.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="trackNumber"/> est hors de la table des pistes SCP.</exception>
    public static (int Cylinder, int Head) ToTrackAddress(int trackNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(trackNumber);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(trackNumber, FloppyTrackSlots);
        return (trackNumber / 2, trackNumber % 2);
    }
}
