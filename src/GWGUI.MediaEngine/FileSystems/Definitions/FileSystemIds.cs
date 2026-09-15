namespace GWGUI.MediaEngine.FileSystems.Definitions;

/// <summary>Définit les identifiants techniques stables des systèmes de fichiers.</summary>
public static class FileSystemIds
{
    public const string Iso9660 = "iso9660";
    public const string Joliet = "iso9660-joliet";
    public const string RockRidge = "iso9660-rock-ridge";
    public const string Udf = "udf";
    /// <summary>Identifies decoded sequential media content exposed for exploration.</summary>
    public const string SequentialContent = "sequential-content";
    /// <summary>Identifie Acorn ADFS/FileCore.</summary>
    public const string AcornAdfs = "acorn-adfs";
    /// <summary>Identifie Acorn DFS.</summary>
    public const string AcornDfs = "acorn-dfs";
    /// <summary>Identifie AmigaDOS.</summary>
    public const string AmigaDos = "amigados";
    /// <summary>Identifie AmigaDOS OFS.</summary>
    public const string AmigaDosOfs = "amigados.ofs";
    /// <summary>Identifie AmigaDOS FFS.</summary>
    public const string AmigaDosFfs = "amigados.ffs";
    /// <summary>Identifie AmigaDOS OFS International.</summary>
    public const string AmigaDosOfsInternational = "amigados.ofs-international";
    /// <summary>Identifie AmigaDOS FFS International.</summary>
    public const string AmigaDosFfsInternational = "amigados.ffs-international";
    /// <summary>Identifie AmigaDOS OFS Directory Cache.</summary>
    public const string AmigaDosOfsDirectoryCache = "amigados.ofs-directory-cache";
    /// <summary>Identifie AmigaDOS FFS Directory Cache.</summary>
    public const string AmigaDosFfsDirectoryCache = "amigados.ffs-directory-cache";
    /// <summary>Identifie AmigaDOS OFS Long Names.</summary>
    public const string AmigaDosOfsLongNames = "amigados.ofs-long-names";
    /// <summary>Identifie AmigaDOS FFS Long Names.</summary>
    public const string AmigaDosFfsLongNames = "amigados.ffs-long-names";
    /// <summary>Identifie une archive Amiga de ressources concaténées décrite par une table linéaire.</summary>
    public const string AmigaFlatResourceArchive = "amiga-flat-resource-archive";
    /// <summary>Identifie la variante Amstrad de CP/M.</summary>
    public const string AmstradCpm = "amstrad.cpm";
    /// <summary>Identifie Apple DOS.</summary>
    public const string AppleDos = "apple-dos";
    /// <summary>Identifie les volumes Apple Inform/XZIP.</summary>
    public const string AppleInformXzip = "apple-inform-xzip";
    /// <summary>Identifie Atari DOS.</summary>
    public const string AtariDos = "atari-dos";
    /// <summary>Identifie une bibliothèque graphique Atari CLK.</summary>
    public const string AtariClkGraphicsLibrary = "atari-clk-graphics-library";
    /// <summary>Identifie une disquette amorçable K-file contenant un exécutable Atari unique.</summary>
    public const string AtariKFile = "atari-k-file";
    /// <summary>Identifie une disquette Atari contenant un flux d'amorçage sans catalogue.</summary>
    public const string AtariBootDisk = "atari-boot-disk";
    /// <summary>Identifie une disquette de données Atari contenant des échantillons PCM 4 bits compactés par demi-octets.</summary>
    public const string AtariPackedPcmDataDisk = "atari-packed-pcm-data-disk";
    /// <summary>Identifie une disquette Atari contenant des images entrelacées en niveaux de gris compactées sur 5 bits.</summary>
    public const string AtariPackedInterlacedGrayscaleDisk = "atari-packed-interlaced-grayscale-disk";
    /// <summary>Identifie un volume Atari contenant des trames d'animation graphiques de longueur fixe.</summary>
    public const string AtariFixedRecordAnimation = "atari-fixed-record-animation";
    /// <summary>Identifie les volumes Atari MyDOS, y compris les grands volumes à liaisons de secteurs sur 16 bits.</summary>
    public const string AtariMyDos = "atari-mydos";
    /// <summary>Identifie un dictionnaire Atari indexé par lettre et compressé par préfixe.</summary>
    public const string AtariFrontCompressedDictionary = "atari-front-compressed-dictionary";
    /// <summary>Identifie Coherent.</summary>
    public const string Coherent = "coherent";
    /// <summary>Identifie Commodore DOS.</summary>
    public const string CommodoreDos = "commodore-dos";
    /// <summary>Identifie CP/M.</summary>
    public const string Cpm = "cpm";
    /// <summary>Identifie FAT12.</summary>
    public const string Fat12 = "fat12";
    /// <summary>Identifie le Lisa Office System.</summary>
    public const string Lisa = "lisa";
    /// <summary>Identifie Macintosh HFS.</summary>
    public const string MacHfs = "mac-hfs";
    /// <summary>Identifie Macintosh MFS.</summary>
    public const string MacMfs = "mac-mfs";
    /// <summary>Identifie ProDOS.</summary>
    public const string ProDos = "prodos";
    /// <summary>Identifie Apple III SOS.</summary>
    public const string Sos = "apple3-sos";
    /// <summary>Identifie RT-11.</summary>
    public const string Rt11 = "rt11";
    /// <summary>Identifie UCSD p-System.</summary>
    public const string Ucsd = "ucsd";
}
