namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Définit les offsets des champs géométriques du BIOS Parameter Block FAT.</summary>
public static class FatBootSectorLayout
{
    /// <summary>Opcode du saut court x86 employé au début d'un secteur d'amorçage DOS.</summary>
    public const byte ShortJumpOpcode = 0xEB;
    /// <summary>Opcode du saut relatif x86 employé au début d'un secteur d'amorçage DOS.</summary>
    public const byte NearJumpOpcode = 0xE9;
    /// <summary>Longueur d'un champ 16 bits du BPB.</summary>
    public const int UInt16Length = sizeof(ushort);
    /// <summary>Longueur d'un champ 32 bits du BPB.</summary>
    public const int UInt32Length = sizeof(uint);
    /// <summary>Longueur minimale requise pour tous les champs utilisés.</summary>
    public const int MinimumLength = 36;
    /// <summary>Taille sectorielle attendue en octets.</summary>
    public const int SectorSize = 512;
    /// <summary>Offset de l'identifiant OEM.</summary>
    public const int OemOffset = 3;
    /// <summary>Longueur de l'identifiant OEM.</summary>
    public const int OemLength = 8;
    /// <summary>Caractère de remplissage nul.</summary>
    public const char NullPadding = '\0';
    /// <summary>Caractère de remplissage espace.</summary>
    public const char SpacePadding = ' ';
    /// <summary>Premier numéro de secteur logique.</summary>
    public const int FirstSectorNumber = 1;
    /// <summary>Indice logique du bloc d'amorçage dans l'image sectorielle.</summary>
    public const int BootLogicalBlock = 0;
    /// <summary>Indice logique du premier bloc FAT dans l'image sectorielle.</summary>
    public const int FirstFatLogicalBlock = 1;
    /// <summary>Numéro de cylindre du secteur d'amorçage et du premier secteur FAT.</summary>
    public const int SystemCylinder = 0;
    /// <summary>Numéro de face du secteur d'amorçage et du premier secteur FAT.</summary>
    public const int SystemHead = 0;
    /// <summary>Numéro du secteur d'amorçage.</summary>
    public const int BootSectorNumber = FirstSectorNumber;
    /// <summary>Numéro du premier secteur FAT.</summary>
    public const int FirstFatSectorNumber = BootSectorNumber + 1;
    /// <summary>Offset du descripteur de média dans les données du premier secteur FAT.</summary>
    public const int FatMediaDescriptorDataOffset = 0;
    /// <summary>Valeur utilisée lorsqu'aucun descripteur de média FAT n'est disponible.</summary>
    public const byte UnknownMediaDescriptor = 0;
    /// <summary>Maximum admis de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 63;
    /// <summary>Maximum admis de têtes.</summary>
    public const int MaximumHeadCount = 2;
    /// <summary>Maximum admis de cylindres.</summary>
    public const int MaximumCylinderCount = 255;
    /// <summary>Décalage du nombre d'octets par secteur.</summary>
    public const int BytesPerSectorOffset = 11;
    /// <summary>Offset du nombre de secteurs par cluster.</summary>
    public const int SectorsPerClusterOffset = 13;
    /// <summary>Offset du nombre de secteurs réservés.</summary>
    public const int ReservedSectorCountOffset = 14;
    /// <summary>Offset du nombre de copies de la FAT.</summary>
    public const int FatCountOffset = 16;
    /// <summary>Offset du nombre d'entrées du répertoire racine.</summary>
    public const int RootEntryCountOffset = 17;
    /// <summary>Décalage du nombre total de secteurs sur 16 bits.</summary>
    public const int TotalSectors16Offset = 19;
    /// <summary>Offset du descripteur de média.</summary>
    public const int MediaDescriptorOffset = 21;
    /// <summary>Offset du nombre de secteurs par FAT.</summary>
    public const int SectorsPerFatOffset = 22;
    /// <summary>Décalage du nombre de secteurs par piste.</summary>
    public const int SectorsPerTrackOffset = 24;
    /// <summary>Décalage du nombre de faces.</summary>
    public const int HeadCountOffset = 26;
    /// <summary>Décalage du nombre total de secteurs sur 32 bits.</summary>
    public const int TotalSectors32Offset = 32;
    /// <summary>Offset du label de volume étendu.</summary>
    public const int VolumeLabelOffset = 43;
    /// <summary>Longueur du label de volume étendu.</summary>
    public const int VolumeLabelLength = 11;
    /// <summary>Longueur minimale nécessaire pour lire le label étendu.</summary>
    public const int ExtendedBootMinimumLength = VolumeLabelOffset + VolumeLabelLength;
    /// <summary>Label indiquant l'absence de nom explicite.</summary>
    public const string EmptyVolumeLabel = "NO NAME";
    /// <summary>Première valeur ASCII imprimable.</summary>
    public const byte PrintableAsciiStart = 0x20;
    /// <summary>Dernière valeur ASCII imprimable.</summary>
    public const byte PrintableAsciiEnd = 0x7e;

    /// <summary>Calcule par arrondi supérieur le nombre de secteurs du répertoire racine.</summary>
    public static int RootDirectorySectorCount(int rootEntries) => checked((rootEntries * FatDirectoryLayout.EntrySize + SectorSize - 1) / SectorSize);
}
