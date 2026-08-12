using GWGUI.MediaEngine.Definitions;

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
    public static string AmigaDos(int dosType) => dosType switch { 0 => "AmigaDOS OFS", 1 => "AmigaDOS FFS", 2 => "AmigaDOS OFS International", 3 => "AmigaDOS FFS International", 4 => "AmigaDOS OFS Directory Cache", 5 => "AmigaDOS FFS Directory Cache", 6 => "AmigaDOS OFS Long Names", 7 => "AmigaDOS FFS Long Names", _ => "AmigaDOS" };
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
    public static string ProDos(string formatId) => formatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) ? "Apple SOS / ProDOS" : "Apple ProDOS";
}
