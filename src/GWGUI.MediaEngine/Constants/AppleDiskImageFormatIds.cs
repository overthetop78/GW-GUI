using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Définit les identifiants des images de disquettes Apple.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe de tous les formats Apple II.</summary>
    public const string AppleIIPrefix = "apple2.";
    /// <summary>Préfixe des formats Apple II AppleDOS.</summary>
    public const string AppleIIAppleDosPrefix = "apple2.appledos";
    /// <summary>Image Apple II AppleDOS de 113 Kio.</summary>
    public const string AppleIIAppleDos113 = MediaImageFormatIds.AppleIIAppleDos113;
    /// <summary>Image Apple II AppleDOS de 140 Kio.</summary>
    public const string AppleIIAppleDos140 = MediaImageFormatIds.AppleIIAppleDos140;
    /// <summary>Préfixe des formats Apple II DOS.</summary>
    public const string AppleIIDosPrefix = "apple2.dos";
    /// <summary>Image Apple II DOS 3.2.</summary>
    public const string AppleIIDos32 = MediaImageFormatIds.AppleIIDos32;
    /// <summary>Image Apple II DOS 3.3.</summary>
    public const string AppleIIDos33 = MediaImageFormatIds.AppleIIDos33;
    /// <summary>Image Apple II GCR non encore classée.</summary>
    public const string AppleIIGcr = "apple2.gcr";
    /// <summary>Préfixe des images Apple II sans système de fichiers imposé.</summary>
    public const string AppleIINoFileSystemPrefix = "apple2.nofs";
    /// <summary>Image Apple II ProDOS générique.</summary>
    public const string AppleIIProDos = MediaImageFormatIds.AppleIIProDos;
    /// <summary>Image Apple II ProDOS de 140 Kio.</summary>
    public const string AppleIIProDos140 = MediaImageFormatIds.AppleIIProDos140;
    /// <summary>Image Apple II ProDOS de 800 Kio.</summary>
    public const string AppleIIProDos800 = MediaImageFormatIds.AppleIIProDos800;
    /// <summary>Image Apple II RWTS18.</summary>
    public const string AppleIIRwts18 = "apple2.rwts18";
    /// <summary>Préfixe des formats Apple III.</summary>
    public const string AppleIIIPrefix = "apple3.";
    /// <summary>Image Apple III SOS.</summary>
    public const string AppleIIISos = MediaImageFormatIds.AppleIIISos;
    /// <summary>Préfixe des formats Apple Lisa.</summary>
    public const string AppleLisaPrefix = "applelisa.";
    /// <summary>Image Apple Lisa MacWorks.</summary>
    public const string AppleLisaMacWorks = "applelisa.macworks";
    /// <summary>Image Apple Lisa Office System.</summary>
    public const string AppleLisaOffice = MediaImageFormatIds.AppleLisaOffice;
    /// <summary>Image sectorielle brute Apple Lisa.</summary>
    public const string AppleLisaRaw = "applelisa.raw";
    /// <summary>Préfixe des formats Apple Macintosh.</summary>
    public const string AppleMacPrefix = "applemac.";
    /// <summary>Image Apple Macintosh GCR non encore classée.</summary>
    public const string AppleMacGcr = "applemac.gcr";
    /// <summary>Image Apple Macintosh HFS.</summary>
    public const string AppleMacHfs = MediaImageFormatIds.AppleMacHfs;
    /// <summary>Image Apple Macintosh MFS.</summary>
    public const string AppleMacMfs = MediaImageFormatIds.AppleMacMfs;
    /// <summary>Préfixe des formats Macintosh bruts.</summary>
    public const string MacPrefix = "mac.";
    /// <summary>Image Macintosh de 400 Kio.</summary>
    public const string Mac400 = MediaImageFormatIds.Mac400;
    /// <summary>Image Macintosh de 800 Kio.</summary>
    public const string Mac800 = MediaImageFormatIds.Mac800;
    /// <summary>Image Macintosh de 1 440 Kio.</summary>
    public const string Mac1440 = MediaImageFormatIds.Mac1440;
}
