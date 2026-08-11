using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Recognition.Definitions;

/// <summary>
/// Regroupe les identifiants stables et les préfixes des formats d’images reconnus par le moteur.
/// </summary>
public static class DiskImageFormatIds
{
    /// <summary>Identifiant utilisé lorsqu’aucun format n’a pu être déterminé.</summary>
    public const string Unknown = "unknown";
    /// <summary>Identifiant générique des conteneurs IMD.</summary>
    public const string Imd = "imd";
    /// <summary>Identifiant générique des conteneurs TD0.</summary>
    public const string Td0 = "td0";

    /// <summary>Préfixe des formats Acorn ADFS.</summary>
    public const string AcornAdfsPrefix = "acorn.adfs.";
    /// <summary>Image Acorn ADFS de 800 Kio.</summary>
    public const string AcornAdfs800 = "acorn.adfs.800";
    /// <summary>Préfixe des formats Acorn DFS.</summary>
    public const string AcornDfsPrefix = "acorn.dfs.";
    /// <summary>Image Acorn DFS simple face de 40 pistes.</summary>
    public const string AcornDfsSingleSided = "acorn.dfs.ss";
    /// <summary>Image Acorn DFS simple face de 80 pistes.</summary>
    public const string AcornDfsSingleSided80 = "acorn.dfs.ss80";
    /// <summary>Image Acorn DFS double face de 40 pistes.</summary>
    public const string AcornDfsDoubleSided = "acorn.dfs.ds";
    /// <summary>Image Acorn DFS double face de 80 pistes.</summary>
    public const string AcornDfsDoubleSided80 = "acorn.dfs.ds80";

    /// <summary>Préfixe des formats Amiga.</summary>
    public const string AmigaPrefix = "amiga.";
    /// <summary>Image AmigaDOS double densité.</summary>
    public const string AmigaDos = "amiga.amigados";
    /// <summary>Image AmigaDOS haute densité.</summary>
    public const string AmigaDosHighDensity = "amiga.amigados_hd";

    /// <summary>Préfixe des formats Amstrad.</summary>
    public const string AmstradPrefix = "amstrad.";
    /// <summary>Image sectorielle Amstrad CPC.</summary>
    public const string AmstradCpc = "amstrad.cpc";
    /// <summary>Image sectorielle Amstrad PCW.</summary>
    public const string AmstradPcw = "amstrad.pcw";

    /// <summary>Préfixe de tous les formats Apple II.</summary>
    public const string AppleIIPrefix = "apple2.";
    /// <summary>Préfixe des formats Apple II AppleDOS.</summary>
    public const string AppleIIAppleDosPrefix = "apple2.appledos";
    /// <summary>Image Apple II AppleDOS de 140 Kio.</summary>
    public const string AppleIIAppleDos140 = "apple2.appledos.140";
    /// <summary>Préfixe des formats Apple II DOS.</summary>
    public const string AppleIIDosPrefix = "apple2.dos";
    /// <summary>Image Apple II DOS 3.2.</summary>
    public const string AppleIIDos32 = "apple2.dos32";
    /// <summary>Image Apple II DOS 3.3.</summary>
    public const string AppleIIDos33 = "apple2.dos33";
    /// <summary>Image Apple II GCR non encore classée.</summary>
    public const string AppleIIGcr = "apple2.gcr";
    /// <summary>Préfixe des images Apple II sans système de fichiers imposé.</summary>
    public const string AppleIINoFileSystemPrefix = "apple2.nofs";
    /// <summary>Image Apple II ProDOS générique.</summary>
    public const string AppleIIProDos = "apple2.prodos";
    /// <summary>Image Apple II ProDOS de 140 Kio.</summary>
    public const string AppleIIProDos140 = "apple2.prodos.140";
    /// <summary>Image Apple II ProDOS de 800 Kio.</summary>
    public const string AppleIIProDos800 = "apple2.prodos.800";
    /// <summary>Image Apple II RWTS18.</summary>
    public const string AppleIIRwts18 = "apple2.rwts18";
    /// <summary>Préfixe des formats Apple III.</summary>
    public const string AppleIIIPrefix = "apple3.";
    /// <summary>Image Apple III SOS.</summary>
    public const string AppleIIISos = "apple3.sos";

    /// <summary>Préfixe des formats Apple Lisa.</summary>
    public const string AppleLisaPrefix = "applelisa.";
    /// <summary>Image Apple Lisa MacWorks.</summary>
    public const string AppleLisaMacWorks = "applelisa.macworks";
    /// <summary>Image Apple Lisa Office System.</summary>
    public const string AppleLisaOffice = "applelisa.office";
    /// <summary>Image sectorielle brute Apple Lisa.</summary>
    public const string AppleLisaRaw = "applelisa.raw";
    /// <summary>Préfixe des formats Apple Macintosh.</summary>
    public const string AppleMacPrefix = "applemac.";
    /// <summary>Image Apple Macintosh GCR non encore classée.</summary>
    public const string AppleMacGcr = "applemac.gcr";
    /// <summary>Image Apple Macintosh HFS.</summary>
    public const string AppleMacHfs = "applemac.hfs";
    /// <summary>Image Apple Macintosh MFS.</summary>
    public const string AppleMacMfs = "applemac.mfs";

    /// <summary>Préfixe des formats Atari 8 bits.</summary>
    public const string AtariPrefix = "atari.";
    /// <summary>Image Atari 8 bits de 90 Kio.</summary>
    public const string Atari90 = "atari.90";
    /// <summary>Image Atari 8 bits de 130 Kio.</summary>
    public const string Atari130 = "atari.130";
    /// <summary>Image Atari 8 bits de 180 Kio.</summary>
    public const string Atari180 = "atari.180";
    /// <summary>Préfixe des formats Atari ST.</summary>
    public const string AtariStPrefix = "atarist.";
    /// <summary>Image Atari ST de 180 Kio.</summary>
    public const string AtariSt180 = "atarist.180";
    /// <summary>Image Atari ST de 360 Kio.</summary>
    public const string AtariSt360 = "atarist.360";
    /// <summary>Image Atari ST de 400 Kio.</summary>
    public const string AtariSt400 = "atarist.400";
    /// <summary>Image Atari ST de 440 Kio.</summary>
    public const string AtariSt440 = "atarist.440";
    /// <summary>Image Atari ST de 720 Kio.</summary>
    public const string AtariSt720 = "atarist.720";
    /// <summary>Image Atari ST de 800 Kio.</summary>
    public const string AtariSt800 = "atarist.800";
    /// <summary>Image Atari ST de 810 Kio.</summary>
    public const string AtariSt810 = "atarist.810";
    /// <summary>Image Atari ST de 880 Kio.</summary>
    public const string AtariSt880 = "atarist.880";
    /// <summary>Image Atari ST de 1 440 Kio.</summary>
    public const string AtariSt1440 = "atarist.1440";

    /// <summary>Préfixe des formats Commodore.</summary>
    public const string CommodorePrefix = "commodore.";
    /// <summary>Image Commodore 1541.</summary>
    public const string Commodore1541 = "commodore.1541";
    /// <summary>Image Commodore 1571.</summary>
    public const string Commodore1571 = "commodore.1571";
    /// <summary>Image Commodore 1581.</summary>
    public const string Commodore1581 = "commodore.1581";
    /// <summary>Préfixe des formats Commodore 900.</summary>
    public const string Commodore900Prefix = "commodore900.";
    /// <summary>Image Commodore 900 utilisant Coherent.</summary>
    public const string Commodore900Coherent = "commodore900.coherent";

    /// <summary>Image DEC RX02.</summary>
    public const string DecRx02 = "dec.rx02";

    /// <summary>Préfixe des formats Epson QX-10.</summary>
    public const string EpsonQx10Prefix = "epson.qx10.";
    /// <summary>Image Epson QX-10 de 320 Kio.</summary>
    public const string EpsonQx10_320 = "epson.qx10.320";
    /// <summary>Image Epson QX-10 de 396 Kio.</summary>
    public const string EpsonQx10_396 = "epson.qx10.396";
    /// <summary>Image Epson QX-10 de 399 Kio.</summary>
    public const string EpsonQx10_399 = "epson.qx10.399";
    /// <summary>Image Epson QX-10 de 400 Kio.</summary>
    public const string EpsonQx10_400 = "epson.qx10.400";
    /// <summary>Image de démarrage Epson QX-10.</summary>
    public const string EpsonQx10Booter = "epson.qx10.booter";
    /// <summary>Image Epson QX-10 LOGO.</summary>
    public const string EpsonQx10Logo = "epson.qx10.logo";

    /// <summary>Préfixe des formats IBM PC.</summary>
    public const string IbmPrefix = "ibm.";
    /// <summary>Image IBM PC de 160 Kio.</summary>
    public const string Ibm160 = "ibm.160";
    /// <summary>Image IBM PC de 180 Kio.</summary>
    public const string Ibm180 = "ibm.180";
    /// <summary>Image IBM PC de 320 Kio.</summary>
    public const string Ibm320 = "ibm.320";
    /// <summary>Image IBM PC de 360 Kio.</summary>
    public const string Ibm360 = "ibm.360";
    /// <summary>Image IBM PC de 720 Kio.</summary>
    public const string Ibm720 = "ibm.720";
    /// <summary>Image IBM PC de 800 Kio.</summary>
    public const string Ibm800 = "ibm.800";
    /// <summary>Image IBM PC de 1 200 Kio.</summary>
    public const string Ibm1200 = "ibm.1200";
    /// <summary>Image IBM PC de 1 440 Kio.</summary>
    public const string Ibm1440 = "ibm.1440";
    /// <summary>Image IBM PC de 1 680 Kio.</summary>
    public const string Ibm1680 = "ibm.1680";
    /// <summary>Image IBM PC au format DMF.</summary>
    public const string IbmDmf = "ibm.dmf";
    /// <summary>Image IBM PC de 2 880 Kio.</summary>
    public const string Ibm2880 = "ibm.2880";
    /// <summary>Image IBM PC dont la géométrie doit être déterminée par analyse.</summary>
    public const string IbmScan = "ibm.scan";

    /// <summary>Préfixe des formats Macintosh bruts.</summary>
    public const string MacPrefix = "mac.";
    /// <summary>Image Macintosh de 400 Kio.</summary>
    public const string Mac400 = "mac.400";
    /// <summary>Image Macintosh de 800 Kio.</summary>
    public const string Mac800 = "mac.800";
    /// <summary>Image Macintosh de 1 440 Kio.</summary>
    public const string Mac1440 = "mac.1440";

    /// <summary>Préfixe des formats MSX.</summary>
    public const string MsxPrefix = "msx.";
    /// <summary>Image MSX 1D.</summary>
    public const string Msx1D = "msx.1d";
    /// <summary>Image MSX 1DD.</summary>
    public const string Msx1Dd = "msx.1dd";
    /// <summary>Image MSX 2D.</summary>
    public const string Msx2D = "msx.2d";
    /// <summary>Image MSX 2DD.</summary>
    public const string Msx2Dd = "msx.2dd";

    /// <summary>Préfixe des formats UCSD.</summary>
    public const string UcsdPrefix = "ucsd.";
    /// <summary>Image UCSD utilisant une géométrie IBM MFM.</summary>
    public const string UcsdIbmMfm = "ucsd.ibm.mfm";

    /// <summary>Construit l’identifiant d’une image Atari ST à partir de sa capacité.</summary>
    /// <param name="capacityBytes">Capacité de l’image, en octets.</param>
    /// <returns>Identifiant Atari ST exprimant la capacité en kibioctets.</returns>
    public static string AtariStFromCapacity(long capacityBytes) => $"{AtariStPrefix}{capacityBytes / DataSizeConstants.BytesPerKibibyte}";

    /// <summary>Construit l’identifiant d’une image IBM PC à partir de sa capacité.</summary>
    /// <param name="capacityBytes">Capacité de l’image, en octets.</param>
    /// <returns>Identifiant IBM PC exprimant la capacité en kibioctets.</returns>
    public static string IbmFromCapacity(long capacityBytes) => $"{IbmPrefix}{capacityBytes / DataSizeConstants.BytesPerKibibyte}";

    /// <summary>Construit l’identifiant de repli d’un conteneur ATR à partir de sa géométrie sectorielle.</summary>
    /// <param name="sectorSize">Taille d’un secteur, en octets.</param>
    /// <param name="sectorCount">Nombre de secteurs du conteneur.</param>
    /// <returns>Identifiant ATR contenant la taille et le nombre de secteurs.</returns>
    public static string AtariAtr(int sectorSize, int sectorCount) => $"atari.atr.{sectorSize}.{sectorCount}";

    /// <summary>Construit l’identifiant de repli d’une reconstruction SCP Atari.</summary>
    /// <param name="sectorSize">Taille d’un secteur reconstruit, en octets.</param>
    /// <param name="sectorsPerTrack">Nombre de secteurs reconstruits par piste.</param>
    /// <returns>Identifiant SCP Atari contenant la taille et le nombre de secteurs par piste.</returns>
    public static string AtariScp(int sectorSize, int sectorsPerTrack) => $"atari.scp.{sectorSize}.{sectorsPerTrack}";
}
