namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Décrit les positions, longueurs et unités binaires du format 86F.</summary>
internal static class I86fLayout
{
    /// <summary>Longueur minimale de l'en-tête, en octets.</summary>
    public const int MinimumFileLength = 8;
    /// <summary>Position des drapeaux du fichier, en octets.</summary>
    public const int FileFlagsOffset = 6;
    /// <summary>Longueur du champ de drapeaux, en octets.</summary>
    public const int FileFlagsLength = 2;
    /// <summary>Position de la table de pistes, en octets.</summary>
    public const int TrackTableOffset = 8;
    /// <summary>Longueur d'une entrée de table, en octets.</summary>
    public const int TrackTableEntrySize = 4;
    /// <summary>Nombre d'entrées de table pour une image à une face.</summary>
    public const int TrackTableEntriesPerSide = 256;
    /// <summary>Nombre d'entrées de table pour une image à deux faces.</summary>
    public const int TwoSideTrackTableEntries = 512;
    /// <summary>Longueur de l'en-tête de piste standard, en octets.</summary>
    public const int StandardTrackHeaderSize = 6;
    /// <summary>Longueur de l'en-tête de piste comportant le champ supplémentaire, en octets.</summary>
    public const int ExtendedTrackHeaderSize = 10;
    /// <summary>Position relative des drapeaux dans un en-tête de piste.</summary>
    public const int TrackFlagsOffset = 0;
    /// <summary>Position relative du nombre explicite de cellules de bits.</summary>
    public const int ExplicitBitCountOffset = 2;
    /// <summary>Alignement du stockage d'une piste, en bits.</summary>
    public const int WordBitAlignment = 16;
    /// <summary>Nombre d'octets par mot de piste.</summary>
    public const int BytesPerWord = 2;
    /// <summary>Nombre de bits par octet.</summary>
    public const int BitsPerByte = 8;
    /// <summary>Masque du premier bit lu dans chaque octet.</summary>
    public const byte MostSignificantBitMask = 0x80;
    /// <summary>Durée d'une cellule de bit dans la représentation de flux, en ticks SCP.</summary>
    public const uint TicksPerBitCell = 40;
}
