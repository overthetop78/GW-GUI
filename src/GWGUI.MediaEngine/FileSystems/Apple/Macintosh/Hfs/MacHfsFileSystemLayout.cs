namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

/// <summary>Définit la disposition binaire d'un volume et d'un catalogue HFS.</summary>
internal static class MacHfsFileSystemLayout
{
    /// <summary>Nom technique affichable du système.</summary>
    public const string SystemName = FileSystems.Definitions.FileSystemDisplayNames.MacHfs;
    /// <summary>Nom technique du fichier catalogue.</summary>
    public const string CatalogName = "catalogue";
    /// <summary>Nom technique du fork de données.</summary>
    public const string DataForkName = "données";
    /// <summary>Nom technique du fork de ressources.</summary>
    public const string ResourceForkName = "ressources";
    /// <summary>Description technique d'un dossier.</summary>
    public const string DirectoryDescription = "Dossier";
    /// <summary>Description d'un fichier sans type Finder.</summary>
    public const string DefaultFileDescription = "Fichier Macintosh";
    /// <summary>Taille d'un secteur HFS.</summary>
    public const int SectorSize = 512;
    /// <summary>Bloc contenant le MDB.</summary>
    public const int MasterDirectoryBlock = 2;
    /// <summary>Longueur minimale du MDB.</summary>
    public const int MinimumMdbLength = 162;
    /// <summary>Signature HFS.</summary>
    public const ushort Signature = 0x4244;
    /// <summary>Offset de la date de création.</summary>
    public const int CreatedOffset = 2;
    /// <summary>Offset de la date de modification.</summary>
    public const int ModifiedOffset = 6;
    /// <summary>Offset du nombre de blocs d'allocation.</summary>
    public const int AllocationCountOffset = 18;
    /// <summary>Offset de la taille d'un bloc d'allocation.</summary>
    public const int AllocationSizeOffset = 20;
    /// <summary>Offset du premier secteur d'allocation.</summary>
    public const int AllocationStartOffset = 28;
    /// <summary>Offset du nombre de blocs d'allocation libres.</summary>
    public const int FreeAllocationCountOffset = 34;
    /// <summary>Offset du nom Pascal du volume.</summary>
    public const int VolumeNameOffset = 36;
    /// <summary>Longueur maximale du nom du volume.</summary>
    public const int MaximumVolumeNameLength = 27;
    /// <summary>Offset de la longueur logique du catalogue.</summary>
    public const int CatalogLengthOffset = 146;
    /// <summary>Offset des extents intégrés du catalogue.</summary>
    public const int CatalogExtentsOffset = 150;
    /// <summary>Longueur des trois extents intégrés.</summary>
    public const int EmbeddedExtentsLength = 12;
    /// <summary>Nombre d'extents intégrés.</summary>
    public const int EmbeddedExtentCount = 3;
    /// <summary>Longueur d'un descripteur d'extent.</summary>
    public const int ExtentDescriptorLength = 4;
    /// <summary>Offset du premier bloc dans un descripteur.</summary>
    public const int ExtentStartOffset = 0;
    /// <summary>Offset du nombre de blocs dans un descripteur.</summary>
    public const int ExtentCountOffset = 2;
    /// <summary>Longueur minimale du fichier catalogue.</summary>
    public const int MinimumCatalogLength = 64;
    /// <summary>Offset de la taille de nœud dans l'en-tête du catalogue.</summary>
    public const int NodeSizeOffset = 32;
    /// <summary>Taille de nœud utilisée lorsque l'en-tête est invalide.</summary>
    public const int DefaultNodeSize = SectorSize;
    /// <summary>Taille minimale d'un nœud.</summary>
    public const int MinimumNodeSize = 256;
    /// <summary>Taille maximale d'un nœud.</summary>
    public const int MaximumNodeSize = 32768;
    /// <summary>Offset du type de nœud.</summary>
    public const int NodeKindOffset = 8;
    /// <summary>Type d'un nœud feuille.</summary>
    public const sbyte LeafNodeKind = -1;
    /// <summary>Offset du nombre de records dans un nœud.</summary>
    public const int RecordCountOffset = 10;
    /// <summary>Longueur minimale du descripteur de nœud.</summary>
    public const int NodeDescriptorLength = 14;
    /// <summary>Nombre maximal de records accepté.</summary>
    public const int MaximumRecordCount = 512;
    /// <summary>Longueur d'une entrée de la table d'offsets.</summary>
    public const int RecordOffsetLength = sizeof(ushort);
    /// <summary>Longueur minimale d'une clé de catalogue.</summary>
    public const int MinimumKeyLength = 6;
    /// <summary>Offset du parent dans une clé.</summary>
    public const int ParentIdOffset = 1;
    /// <summary>Offset de la longueur du nom dans une clé.</summary>
    public const int NameLengthOffset = 5;
    /// <summary>Offset du nom dans une clé.</summary>
    public const int NameOffset = 6;
    /// <summary>Longueur maximale d'un nom HFS.</summary>
    public const int MaximumNameLength = 31;
    /// <summary>Alignement des données d'un record.</summary>
    public const int RecordAlignment = 2;
    /// <summary>Type d'un record dossier.</summary>
    public const byte DirectoryRecordType = 1;
    /// <summary>Type d'un record fichier.</summary>
    public const byte FileRecordType = 2;
    /// <summary>Longueur minimale d'un record dossier.</summary>
    public const int MinimumDirectoryRecordLength = 70;
    /// <summary>Offset de l'identifiant d'un dossier.</summary>
    public const int DirectoryIdOffset = 6;
    /// <summary>Offset de modification d'un dossier.</summary>
    public const int DirectoryModifiedOffset = 14;
    /// <summary>Longueur minimale d'un record fichier.</summary>
    public const int MinimumFileRecordLength = 102;
    /// <summary>Offset du type Finder.</summary>
    public const int FinderTypeOffset = 4;
    /// <summary>Longueur du type Finder.</summary>
    public const int FinderTypeLength = 4;
    /// <summary>Offset de l'identifiant d'un fichier.</summary>
    public const int FileIdOffset = 20;
    /// <summary>Offset de la longueur du data fork.</summary>
    public const int DataForkLengthOffset = 26;
    /// <summary>Offset de la longueur du resource fork.</summary>
    public const int ResourceForkLengthOffset = 36;
    /// <summary>Offset de modification d'un fichier.</summary>
    public const int FileModifiedOffset = 48;
    /// <summary>Offset des extents du data fork.</summary>
    public const int DataForkExtentsOffset = 74;
    /// <summary>Offset des extents du resource fork.</summary>
    public const int ResourceForkExtentsOffset = 86;
    /// <summary>Identifiant du dossier racine HFS.</summary>
    public const uint RootDirectoryId = 2;
}
