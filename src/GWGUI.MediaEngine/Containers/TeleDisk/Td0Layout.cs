namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Décrit les positions, tailles, limites et marqueurs binaires de TeleDisk.</summary>
internal static class Td0Layout
{
    /// <summary>Longueur de l'en-tête global, en octets.</summary>
    public const int HeaderSize = 12;
    /// <summary>Position de la signature, en octets.</summary>
    public const int SignatureOffset = 0;
    /// <summary>Position du numéro de volume.</summary>
    public const int SequenceOffset = 2;
    /// <summary>Position de la signature commune d'un jeu multi-volume.</summary>
    public const int CheckSignatureOffset = 3;
    /// <summary>Longueur d'un champ d'un octet.</summary>
    public const int ByteFieldSize = 1;
    /// <summary>Position de la version, en octets.</summary>
    public const int VersionOffset = 4;
    /// <summary>Position du mode de données, en octets.</summary>
    public const int DataRateOffset = 5;
    /// <summary>Position du type de lecteur source.</summary>
    public const int DriveTypeOffset = 6;
    /// <summary>Position du stepping et du drapeau de commentaire, en octets.</summary>
    public const int SteppingOffset = 7;
    /// <summary>Position de l'indicateur d'analyse de l'allocation DOS.</summary>
    public const int DosModeOffset = 8;
    /// <summary>Position du nombre de faces enregistrées.</summary>
    public const int SurfaceCountOffset = 9;
    /// <summary>Position du CRC global, en octets.</summary>
    public const int HeaderCrcOffset = 10;
    /// <summary>Longueur de l'en-tête de commentaire, en octets.</summary>
    public const int CommentHeaderSize = 10;
    /// <summary>Position de la longueur du commentaire dans son en-tête.</summary>
    public const int CommentLengthOffset = 2;
    /// <summary>Position de l'année dans l'en-tête de commentaire.</summary>
    public const int CommentYearOffset = 4;
    /// <summary>Position du mois dans l'en-tête de commentaire.</summary>
    public const int CommentMonthOffset = 5;
    /// <summary>Position du jour dans l'en-tête de commentaire.</summary>
    public const int CommentDayOffset = 6;
    /// <summary>Position de l'heure dans l'en-tête de commentaire.</summary>
    public const int CommentHourOffset = 7;
    /// <summary>Position des minutes dans l'en-tête de commentaire.</summary>
    public const int CommentMinuteOffset = 8;
    /// <summary>Position des secondes dans l'en-tête de commentaire.</summary>
    public const int CommentSecondOffset = 9;
    /// <summary>Position du CRC du commentaire dans son en-tête.</summary>
    public const int CommentCrcOffset = 0;
    /// <summary>Longueur d'un en-tête de piste, en octets.</summary>
    public const int TrackHeaderSize = 4;
    /// <summary>Position du nombre de secteurs dans l'en-tête de piste.</summary>
    public const int TrackSectorCountOffset = 0;
    /// <summary>Position du cylindre physique dans l'en-tête de piste.</summary>
    public const int TrackCylinderOffset = 1;
    /// <summary>Position de la face physique dans l'en-tête de piste.</summary>
    public const int TrackHeadOffset = 2;
    /// <summary>Position du CRC dans l'en-tête de piste.</summary>
    public const int TrackCrcOffset = 3;
    /// <summary>Longueur d'un en-tête de secteur, en octets.</summary>
    public const int SectorHeaderSize = 6;
    /// <summary>Position du cylindre logique dans l'en-tête de secteur.</summary>
    public const int SectorCylinderOffset = 0;
    /// <summary>Position de la face logique dans l'en-tête de secteur.</summary>
    public const int SectorHeadOffset = 1;
    /// <summary>Position du numéro dans l'en-tête de secteur.</summary>
    public const int SectorNumberOffset = 2;
    /// <summary>Position du code de taille dans l'en-tête de secteur.</summary>
    public const int SectorSizeCodeOffset = 3;
    /// <summary>Position des drapeaux dans l'en-tête de secteur.</summary>
    public const int SectorFlagsOffset = 4;
    /// <summary>Position du CRC dans l'en-tête de secteur.</summary>
    public const int SectorCrcOffset = 5;
    /// <summary>Longueur de l'en-tête de données sectorielles, en octets.</summary>
    public const int SectorDataHeaderSize = 3;
    /// <summary>Position de la longueur encodée dans l'en-tête de données.</summary>
    public const int EncodedLengthOffset = 0;
    /// <summary>Position de l'encodage dans l'en-tête de données.</summary>
    public const int EncodingOffset = 2;
    /// <summary>Longueur du champ d'encodage incluse dans la longueur déclarée.</summary>
    public const int EncodingFieldSize = 1;
    /// <summary>Longueur d'un mot TeleDisk, en octets.</summary>
    public const int WordSize = 2;
    /// <summary>Taille sectorielle correspondant au code zéro, en octets.</summary>
    public const int BaseSectorSize = 128;
    /// <summary>Code maximal de taille sectorielle accepté.</summary>
    public const int MaximumSectorSizeCode = 6;
    /// <summary>Masque du numéro de face.</summary>
    public const int HeadMask = 0x01;
    /// <summary>Drapeau indiquant la présence d'un commentaire.</summary>
    public const byte CommentPresentMask = 0x80;
    /// <summary>Marqueur terminant la liste des pistes.</summary>
    public const byte EndOfTracks = 0xFF;
    /// <summary>Longueur d'une charge utile à motif de mot répété.</summary>
    public const int RepeatedSectorPayloadSize = 4;
    /// <summary>Position du compteur dans un motif de mot répété.</summary>
    public const int RepeatedSectorCountOffset = 0;
    /// <summary>Position du premier octet du motif répété.</summary>
    public const int RepeatedSectorPatternOffset = 2;
    /// <summary>Position du second octet du motif répété.</summary>
    public const int RepeatedSectorSecondPatternByteOffset = 3;
    /// <summary>Longueur du contrôle d'une séquence RLE, en octets.</summary>
    public const int RleControlSize = 2;
    /// <summary>Nombre d'octets par mot dans un motif RLE.</summary>
    public const int PatternWordSize = 2;
}
