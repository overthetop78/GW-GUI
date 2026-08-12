using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Décrit les tailles et positions fixes utilisées dans les conteneurs WOZ1 et WOZ2.</summary>
internal static class WozLayout
{
    /// <summary>Taille minimale, en octets, acceptée pour un conteneur WOZ.</summary>
    public const int MinimumFileLength = 256;
    /// <summary>Longueur, en octets, de la signature WOZ.</summary>
    public const int SignatureLength = 4;
    /// <summary>Position de la marque binaire dans l’en-tête WOZ.</summary>
    public const int HeaderMarkerOffset = SignatureLength;
    /// <summary>Longueur, en octets, de la marque binaire.</summary>
    public const int HeaderMarkerLength = 4;
    /// <summary>Position du CRC32 stocké dans l’en-tête.</summary>
    public const int CrcOffset = HeaderMarkerOffset + HeaderMarkerLength;
    /// <summary>Longueur, en octets, du CRC32.</summary>
    public const int CrcLength = 4;
    /// <summary>Position du premier chunk et début des octets couverts par le CRC32.</summary>
    public const int ChunksOffset = CrcOffset + CrcLength;
    /// <summary>Longueur de l’en-tête précédant chaque charge utile de chunk.</summary>
    public const int ChunkHeaderLength = ChunkLengthOffset + ChunkLengthSize;
    /// <summary>Position de l’identifiant dans l’en-tête d’un chunk.</summary>
    public const int ChunkIdOffset = 0;
    /// <summary>Longueur, en octets, d’un identifiant de chunk.</summary>
    public const int ChunkIdLength = 4;
    /// <summary>Position de la longueur dans l’en-tête d’un chunk.</summary>
    public const int ChunkLengthOffset = ChunkIdOffset + ChunkIdLength;
    /// <summary>Longueur, en octets, du champ de longueur d’un chunk.</summary>
    public const int ChunkLengthSize = 4;
    /// <summary>Longueur minimale du chunk INFO nécessaire à la lecture du type de disque.</summary>
    public const int MinimumInfoLength = 2;
    /// <summary>Longueur du chunk INFO produit par le Writer WOZ1.</summary>
    public const int InfoLength = 60;
    /// <summary>Position de la version INFO.</summary>
    public const int InfoVersionOffset = 0;
    /// <summary>Position du type de disque dans le chunk INFO.</summary>
    public const int InfoDiskTypeOffset = 1;
    /// <summary>Position du drapeau de protection en écriture.</summary>
    public const int InfoWriteProtectionOffset = 2;
    /// <summary>Position du drapeau de synchronisation des pistes.</summary>
    public const int InfoSynchronizedOffset = 3;
    /// <summary>Position du drapeau indiquant une image nettoyée.</summary>
    public const int InfoCleanedOffset = 4;
    /// <summary>Position du nom du créateur.</summary>
    public const int InfoCreatorOffset = 5;
    /// <summary>Longueur attendue du chunk TMAP.</summary>
    public const int TrackMapLength = AppleIITrackCount * TrackMapEntriesPerTrack;
    /// <summary>Nombre de pistes Apple II examinées.</summary>
    public const int AppleIITrackCount = DiskGeometryConstants.FortyTrackCylinderCount;
    /// <summary>Index de la première piste Apple II examinée.</summary>
    public const int FirstAppleIITrackIndex = 0;
    /// <summary>Nombre d’entrées TMAP examinées pour chaque piste Apple II.</summary>
    public const int TrackMapEntriesPerTrack = 4;
    /// <summary>Valeur TMAP indiquant qu’aucun descripteur n’est associé.</summary>
    public const byte MissingTrackDescriptor = 0xff;
    /// <summary>Taille fixe, en octets, d’une entrée de piste WOZ1.</summary>
    public const int Woz1TrackEntryLength = NibLayout.TrackLengthBytes;
    /// <summary>Position du nombre de bits dans une entrée de piste WOZ1.</summary>
    public const int Woz1BitCountOffset = 6648;
    /// <summary>Longueur du champ de nombre de bits WOZ1.</summary>
    public const int Woz1BitCountLength = 2;
    /// <summary>Nombre maximal de bits stockables dans les données d'une piste WOZ1.</summary>
    public const int Woz1MaximumBitCount = Woz1BitCountOffset * BitPrimitives.BitsPerByte;
    /// <summary>Taille d’un bloc de données WOZ2.</summary>
    public const int Woz2BlockLength = 512;
    /// <summary>Taille d’un descripteur de piste WOZ2.</summary>
    public const int Woz2TrackDescriptorLength = Woz2BitCountOffset + Woz2BitCountLength;
    /// <summary>Position du premier bloc dans un descripteur WOZ2.</summary>
    public const int Woz2StartBlockOffset = 0;
    /// <summary>Valeur indiquant l'absence de bloc de départ WOZ2.</summary>
    public const int MissingWoz2StartBlock = 0;
    /// <summary>Position du nombre de blocs dans un descripteur WOZ2.</summary>
    public const int Woz2BlockCountOffset = Woz2StartBlockOffset + Woz2BlockFieldLength;
    /// <summary>Position du nombre de bits dans un descripteur WOZ2.</summary>
    public const int Woz2BitCountOffset = Woz2BlockCountOffset + Woz2BlockFieldLength;
    /// <summary>Longueur des champs premier bloc et nombre de blocs WOZ2.</summary>
    public const int Woz2BlockFieldLength = 2;
    /// <summary>Longueur du champ de nombre de bits WOZ2.</summary>
    public const int Woz2BitCountLength = 4;
    /// <summary>Valeur indiquant qu'une piste ne contient aucun bit.</summary>
    public const int EmptyTrackBitCount = 0;
    /// <summary>Valeur indiquant qu'un descripteur WOZ2 ne référence aucun bloc.</summary>
    public const int EmptyWoz2BlockCount = 0;
}
