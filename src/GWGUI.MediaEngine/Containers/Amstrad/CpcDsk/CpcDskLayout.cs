namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>
/// Décrit les tailles, offsets, limites et masques de la disposition binaire CPCEMU DSK.
/// Tous les offsets sont exprimés en octets depuis le début du bloc auquel ils appartiennent.
/// </summary>
public static class CpcDskLayout
{
    /// <summary>Taille, en octets, du bloc d’informations disque situé au début du conteneur.</summary>
    public const int DiskInformationBlockSize = 0x100;

    /// <summary>Taille, en octets, du bloc d’informations placé au début de chaque piste.</summary>
    public const int TrackInformationBlockSize = 0x100;

    /// <summary>Nombre d’octets lus au début du bloc d’informations disque pour reconnaître sa signature.</summary>
    public const int DiskSignatureLength = 34;

    /// <summary>Offset du champ ASCII identifiant le logiciel créateur dans le bloc d’informations disque.</summary>
    public const int CreatorOffset = 34;

    /// <summary>Longueur, en octets, du champ ASCII identifiant le logiciel créateur.</summary>
    public const int CreatorLength = 14;

    /// <summary>Offset du nombre de cylindres dans le bloc d’informations disque.</summary>
    public const int CylinderCountOffset = 48;

    /// <summary>Offset du nombre de faces dans le bloc d’informations disque.</summary>
    public const int HeadCountOffset = 49;

    /// <summary>Offset de la taille commune des pistes Standard dans le bloc d’informations disque.</summary>
    public const int StandardTrackSizeOffset = 50;

    /// <summary>Offset de la table des tailles de pistes Extended dans le bloc d’informations disque.</summary>
    public const int ExtendedTrackSizeTableOffset = 52;

    /// <summary>Unité, en octets, appliquée à chaque valeur de la table des tailles de pistes Extended.</summary>
    public const int ExtendedTrackSizeUnit = 256;

    /// <summary>Nombre maximal de cylindres accepté dans la géométrie déclarée.</summary>
    public const int MaximumCylinderCount = 168;

    /// <summary>Nombre maximal de faces accepté dans la géométrie déclarée.</summary>
    public const int MaximumHeadCount = 2;

    /// <summary>Nombre d’octets lus au début d’une piste pour reconnaître sa signature.</summary>
    public const int TrackSignatureLength = 12;

    /// <summary>Offset du numéro de cylindre dans le bloc d’informations de piste.</summary>
    public const int TrackCylinderOffset = 16;

    /// <summary>Offset du numéro de face dans le bloc d’informations de piste.</summary>
    public const int TrackHeadOffset = 17;

    /// <summary>Offset du code de taille sectorielle par défaut dans le bloc d’informations de piste.</summary>
    public const int TrackSectorSizeCodeOffset = 20;

    /// <summary>Offset du nombre de secteurs dans le bloc d’informations de piste.</summary>
    public const int TrackSectorCountOffset = 21;

    /// <summary>Offset de la longueur GAP#3 dans le bloc d’informations de piste.</summary>
    public const int TrackGap3LengthOffset = 22;

    /// <summary>Offset de l’octet de remplissage dans le bloc d’informations de piste.</summary>
    public const int TrackFillerByteOffset = 23;

    /// <summary>Offset du premier descripteur de secteur dans le bloc d’informations de piste.</summary>
    public const int SectorDescriptorTableOffset = 24;

    /// <summary>Taille, en octets, d’un descripteur de secteur.</summary>
    public const int SectorDescriptorSize = 8;

    /// <summary>Offset du cylindre dans un descripteur de secteur.</summary>
    public const int SectorCylinderOffset = 0;

    /// <summary>Offset de la face dans un descripteur de secteur.</summary>
    public const int SectorHeadOffset = 1;

    /// <summary>Offset de l’identifiant de secteur dans un descripteur de secteur.</summary>
    public const int SectorIdOffset = 2;

    /// <summary>Offset du code de taille dans un descripteur de secteur.</summary>
    public const int SectorSizeCodeOffset = 3;

    /// <summary>Offset du premier octet d’état dans un descripteur de secteur.</summary>
    public const int SectorStatus1Offset = 4;

    /// <summary>Offset du second octet d’état dans un descripteur de secteur.</summary>
    public const int SectorStatus2Offset = 5;

    /// <summary>Offset de la taille stockée dans un descripteur de secteur Extended.</summary>
    public const int SectorStoredSizeOffset = 6;

    /// <summary>Longueur, en octets, d’un champ de taille stockée sur 16 bits.</summary>
    public const int StoredSizeFieldLength = 2;

    /// <summary>Taille sectorielle minimale, en octets, avant application du code de taille.</summary>
    public const int MinimumSectorSize = 128;

    /// <summary>Masque isolant les trois bits du code de taille sectorielle.</summary>
    public const int SectorSizeCodeMask = 0x07;

    /// <summary>Masque du bit signalant une erreur d’intégrité des données dans le premier octet d’état.</summary>
    public const int DataErrorMask = 0x20;
}
