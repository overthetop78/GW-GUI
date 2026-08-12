namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Définit la disposition sectorielle des répertoires Atari DOS.</summary>
public static class AtariDosFileSystemLayout
{
    /// <summary>Numéro du secteur VTOC, compté depuis un.</summary>
    public const int VtocSector = 360;
    /// <summary>Premier secteur du catalogue, compté depuis un.</summary>
    public const int FirstDirectorySector = 361;
    /// <summary>Nombre de secteurs de catalogue.</summary>
    public const int DirectorySectorCount = 8;
    /// <summary>Dernier secteur du catalogue.</summary>
    public const int LastDirectorySector = FirstDirectorySector + DirectorySectorCount - 1;
    /// <summary>Taille sectorielle minimale.</summary>
    public const int MinimumSectorSize = 128;
    /// <summary>Taille d'une entrée.</summary>
    public const int DirectoryEntrySize = 16;
    /// <summary>Nombre d'entrées par secteur.</summary>
    public const int DirectoryEntriesPerSector = 8;
    /// <summary>Offset des drapeaux.</summary>
    public const int FlagsOffset = 0;
    /// <summary>Offset du compteur de secteurs.</summary>
    public const int SectorCountOffset = 1;
    /// <summary>Offset du premier secteur.</summary>
    public const int FirstSectorOffset = 3;
    /// <summary>Offset du nom.</summary>
    public const int NameOffset = 5;
    /// <summary>Longueur du radical.</summary>
    public const int NameLength = 8;
    /// <summary>Longueur de l'extension.</summary>
    public const int ExtensionLength = 3;
    /// <summary>Drapeau d'une entrée utilisée.</summary>
    public const byte InUseFlag = 0x40;
    /// <summary>Drapeau d'une entrée supprimée.</summary>
    public const byte DeletedFlag = 0x80;
    /// <summary>Nombre d'octets de liaison à la fin d'un secteur.</summary>
    public const int LinkByteCount = 3;
    /// <summary>Décalage du propriétaire de fichier.</summary>
    public const int FileOwnerShift = 2;
    /// <summary>Masque des bits hauts du secteur suivant.</summary>
    public const byte NextSectorHighMask = 3;
    /// <summary>Offset du compteur libre dans le VTOC.</summary>
    public const int FreeSectorCountOffset = 3;
    /// <summary>Marqueur VTOC reconnu.</summary>
    public const byte VtocMarker = 2;
    /// <summary>Largeur du compteur libre.</summary>
    public const int FreeSectorCountLength = sizeof(ushort);
    /// <summary>Caractère de remplissage des noms.</summary>
    public const byte NamePadding = 0x20;
    /// <summary>Premier caractère ASCII imprimable.</summary>
    public const byte MinimumNameCharacter = 0x20;
    /// <summary>Dernier caractère ASCII imprimable.</summary>
    public const byte MaximumNameCharacter = 0x7e;
}
