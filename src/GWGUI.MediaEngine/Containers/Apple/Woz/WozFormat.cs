namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Regroupe les signatures et identifiants définissant un conteneur WOZ.</summary>
internal static class WozFormat
{
    /// <summary>Signature d’un conteneur WOZ version 1.</summary>
    public static ReadOnlySpan<byte> Version1Signature => "WOZ1"u8;
    /// <summary>Signature d’un conteneur WOZ version 2.</summary>
    public static ReadOnlySpan<byte> Version2Signature => "WOZ2"u8;
    /// <summary>Marque binaire suivant immédiatement la signature WOZ.</summary>
    public static ReadOnlySpan<byte> HeaderMarker => [0xff, 0x0a, 0x0d, 0x0a];
    /// <summary>Identifiant du chunk d’informations générales.</summary>
    public const string InfoChunkId = "INFO";
    /// <summary>Identifiant du chunk de correspondance des pistes.</summary>
    public const string TrackMapChunkId = "TMAP";
    /// <summary>Identifiant du chunk contenant les pistes ou leurs descripteurs.</summary>
    public const string TracksChunkId = "TRKS";
    /// <summary>Type de disque WOZ représentant une disquette Apple II 5,25 pouces.</summary>
    public const byte AppleII525DiskType = 1;
    /// <summary>Version du chunk INFO produit pour WOZ1.</summary>
    public const byte InfoVersion1 = 1;
    /// <summary>Valeur indiquant une image non protégée en écriture.</summary>
    public const byte Writable = 0;
    /// <summary>Valeur indiquant des pistes synchronisées.</summary>
    public const byte Synchronized = 1;
    /// <summary>Valeur indiquant une image nettoyée.</summary>
    public const byte Cleaned = 1;
    /// <summary>Nom du logiciel créateur inscrit dans INFO.</summary>
    public const string Creator = "GW GUI";
    /// <summary>Polynôme inversé utilisé par le CRC32 du conteneur WOZ.</summary>
    public const uint Crc32Polynomial = 0xedb88320u;
}
