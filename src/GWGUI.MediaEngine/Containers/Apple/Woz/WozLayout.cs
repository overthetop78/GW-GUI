using GWGUI.MediaEngine.Recognition.Apple;

namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Décrit les tailles et positions fixes utilisées dans les conteneurs WOZ1 et WOZ2.</summary>
internal static class WozLayout
{
    /// <summary>Taille minimale, en octets, acceptée pour un conteneur WOZ.</summary>
    public const int MinimumFileLength = 256;
    /// <summary>Longueur, en octets, de la signature WOZ.</summary>
    public const int SignatureLength = 4;
    /// <summary>Position de la marque binaire dans l’en-tête WOZ.</summary>
    public const int HeaderMarkerOffset = 4;
    /// <summary>Longueur, en octets, de la marque binaire.</summary>
    public const int HeaderMarkerLength = 4;
    /// <summary>Position du CRC32 stocké dans l’en-tête.</summary>
    public const int CrcOffset = 8;
    /// <summary>Longueur, en octets, du CRC32.</summary>
    public const int CrcLength = 4;
    /// <summary>Position du premier chunk et début des octets couverts par le CRC32.</summary>
    public const int ChunksOffset = 12;
    /// <summary>Longueur de l’en-tête précédant chaque charge utile de chunk.</summary>
    public const int ChunkHeaderLength = 8;
    /// <summary>Position de l’identifiant dans l’en-tête d’un chunk.</summary>
    public const int ChunkIdOffset = 0;
    /// <summary>Longueur, en octets, d’un identifiant de chunk.</summary>
    public const int ChunkIdLength = 4;
    /// <summary>Position de la longueur dans l’en-tête d’un chunk.</summary>
    public const int ChunkLengthOffset = 4;
    /// <summary>Longueur, en octets, du champ de longueur d’un chunk.</summary>
    public const int ChunkLengthSize = 4;
    /// <summary>Longueur minimale du chunk INFO nécessaire à la lecture du type de disque.</summary>
    public const int MinimumInfoLength = 2;
    /// <summary>Position du type de disque dans le chunk INFO.</summary>
    public const int InfoDiskTypeOffset = 1;
    /// <summary>Longueur attendue du chunk TMAP.</summary>
    public const int TrackMapLength = 160;
    /// <summary>Nombre de pistes Apple II examinées.</summary>
    public const int AppleIITrackCount = 40;
    /// <summary>Nombre d’entrées TMAP examinées pour chaque piste Apple II.</summary>
    public const int TrackMapEntriesPerTrack = 4;
    /// <summary>Valeur TMAP indiquant qu’aucun descripteur n’est associé.</summary>
    public const byte MissingTrackDescriptor = 0xff;
    /// <summary>Taille fixe, en octets, d’une entrée de piste WOZ1.</summary>
    public const int Woz1TrackEntryLength = NibTrackFormat.TrackLength;
    /// <summary>Position du nombre de bits dans une entrée de piste WOZ1.</summary>
    public const int Woz1BitCountOffset = 6648;
    /// <summary>Longueur du champ de nombre de bits WOZ1.</summary>
    public const int Woz1BitCountLength = 2;
    /// <summary>Taille d’un bloc de données WOZ2.</summary>
    public const int Woz2BlockLength = 512;
    /// <summary>Taille d’un descripteur de piste WOZ2.</summary>
    public const int Woz2TrackDescriptorLength = 8;
    /// <summary>Position du premier bloc dans un descripteur WOZ2.</summary>
    public const int Woz2StartBlockOffset = 0;
    /// <summary>Position du nombre de blocs dans un descripteur WOZ2.</summary>
    public const int Woz2BlockCountOffset = 2;
    /// <summary>Position du nombre de bits dans un descripteur WOZ2.</summary>
    public const int Woz2BitCountOffset = 4;
    /// <summary>Longueur des champs premier bloc et nombre de blocs WOZ2.</summary>
    public const int Woz2BlockFieldLength = 2;
    /// <summary>Longueur du champ de nombre de bits WOZ2.</summary>
    public const int Woz2BitCountLength = 4;
}
