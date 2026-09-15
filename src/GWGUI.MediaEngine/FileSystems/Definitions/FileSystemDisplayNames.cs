using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.FileSystems.Definitions;

/// <summary>Définit les noms affichés des systèmes de fichiers reconnus.</summary>
internal static class FileSystemDisplayNames
{
    /// <summary>Nom affiché d'Acorn ADFS.</summary>
    public const string AcornAdfs = "Acorn ADFS";
    /// <summary>Nom affiché d'Acorn DFS.</summary>
    public const string AcornDfs = "Acorn DFS";
    /// <summary>Nom affiché d'Apple DOS 3.2.</summary>
    public const string AppleDos32 = "Apple DOS 3.2";
    /// <summary>Nom affiché d'Apple DOS 3.3.</summary>
    public const string AppleDos33 = "Apple DOS 3.3";
    /// <summary>Nom affiché des volumes Inform/XZIP.</summary>
    public const string AppleInformXzip = "Apple II Inform/XZIP";
    /// <summary>Nom affiché d'Atari DOS.</summary>
    public const string AtariDos = "Atari DOS";
    /// <summary>Nom affiché d'une bibliothèque graphique Atari CLK.</summary>
    public const string AtariClkGraphicsLibrary = "Atari CLK graphics library";
    /// <summary>Nom affiché d'une disquette amorçable Atari K-file.</summary>
    public const string AtariKFile = "Atari K-file";
    /// <summary>Nom affiché d'une disquette Atari contenant un flux d'amorçage sans catalogue.</summary>
    public const string AtariBootDisk = "Atari boot disk";
    /// <summary>Nom affiché d'une disquette de données Atari contenant des échantillons PCM 4 bits compactés.</summary>
    public const string AtariPackedPcmDataDisk = "Atari packed PCM data disk";
    /// <summary>Nom affiché d'une disquette Atari contenant des images entrelacées en niveaux de gris compactées sur 5 bits.</summary>
    public const string AtariPackedInterlacedGrayscaleDisk = "Atari packed interlaced grayscale disk";
    /// <summary>Nom affiché d'un volume Atari contenant des trames d'animation graphiques de longueur fixe.</summary>
    public const string AtariFixedRecordAnimation = "Atari fixed-record animation";
    /// <summary>Nom affiché des volumes Atari MyDOS.</summary>
    public const string AtariMyDos = "Atari MyDOS";
    /// <summary>Nom affiché d'un dictionnaire Atari indexé par lettre et compressé par préfixe.</summary>
    public const string AtariFrontCompressedDictionary = "Atari front-compressed dictionary";
    /// <summary>Nom affiché de Coherent sur Commodore 900.</summary>
    public const string CoherentCommodore900 = "COHERENT (Commodore 900)";
    /// <summary>Nom affiché de Commodore DOS.</summary>
    public const string CommodoreDos = "CBM DOS";
    /// <summary>Nom affiché de CP/M 3.</summary>
    public const string Cpm3 = "CP/M 3";
    /// <summary>Nom affiché de Macintosh HFS.</summary>
    public const string MacHfs = "Macintosh HFS";
    /// <summary>Nom affiché de Macintosh MFS.</summary>
    public const string MacMfs = "Macintosh MFS";
    /// <summary>Nom affiché de DEC RT-11.</summary>
    public const string Rt11 = "DEC RT-11";
    /// <summary>Nom affiché d'UCSD p-System.</summary>
    public const string Ucsd = "UCSD p-System";

    /// <summary>Retourne le nom de la variante AmigaDOS correspondant à son octet DOS.</summary>
    /// <param name="dosType">Octet identifiant la variante AmigaDOS.</param>
    /// <returns>Nom affiché de la variante AmigaDOS.</returns>
    public static string AmigaDos(AmigaDosVariant variant) => variant switch { AmigaDosVariant.Ofs => "AmigaDOS OFS", AmigaDosVariant.Ffs => "AmigaDOS FFS", AmigaDosVariant.OfsInternational => "AmigaDOS OFS International", AmigaDosVariant.FfsInternational => "AmigaDOS FFS International", AmigaDosVariant.OfsDirectoryCache => "AmigaDOS OFS Directory Cache", AmigaDosVariant.FfsDirectoryCache => "AmigaDOS FFS Directory Cache", AmigaDosVariant.OfsLongNames => "AmigaDOS OFS Long Names", AmigaDosVariant.FfsLongNames => "AmigaDOS FFS Long Names", _ => "AmigaDOS" };
    /// <summary>Retourne le nom de la variante CP/M Amstrad correspondant au format.</summary>
    /// <param name="formatId">Identifiant du format d'image disque.</param>
    /// <returns>Nom affiché de la variante CP/M Amstrad.</returns>
    public static string AmstradCpm(string formatId) => formatId.Equals(DiskImageFormatIds.AmstradPcw, StringComparison.OrdinalIgnoreCase) ? "Amstrad PCW CP/M Plus" : "Amstrad CPC CP/M";
    /// <summary>Retourne le nom de la variante FAT12 correspondant au format.</summary>
    /// <param name="formatId">Identifiant du format d'image disque.</param>
    /// <returns>Nom affiché de la variante FAT12.</returns>
    public static string Fat12(string formatId) => formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) ? "IBM PC FAT12" : formatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) ? "MSX-DOS FAT12" : "Atari TOS FAT12";
    /// <summary>Retourne le nom de la variante Lisa correspondant à sa version de catalogue.</summary>
    /// <param name="version">Version du catalogue Lisa.</param>
    /// <returns>Nom affiché de la variante Lisa.</returns>
    public static string Lisa(ushort version) => version switch { 0x000e => "Lisa Office System (table catalog)", 0x000f => "Lisa Office System (hash catalog)", 0x0011 => "Lisa Office System (B-tree catalog)", _ => $"Lisa Office System (${version:X4})" };
    /// <summary>Retourne le nom ProDOS ou SOS correspondant au format.</summary>
    /// <param name="formatId">Identifiant du format d'image disque.</param>
    /// <returns>Nom affiché de ProDOS ou SOS.</returns>
    public static string ProDos(string formatId) => formatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase) ? AppleSosProDos : AppleProDos;
    /// <summary>Nom affiché d'un volume Apple III SOS lu par le moteur ProDOS.</summary>
    public const string AppleSosProDos = "Apple SOS / ProDOS";
    /// <summary>Nom affiché d'un volume Apple ProDOS.</summary>
    public const string AppleProDos = "Apple ProDOS";
}
