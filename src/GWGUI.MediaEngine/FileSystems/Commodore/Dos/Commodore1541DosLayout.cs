namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Définit les secteurs système communs aux volumes 1541 et 1571.</summary>
public static class Commodore1541DosLayout
{
    /// <summary>Piste de l'en-tête et du BAM.</summary>
    public const int HeaderTrack = 18;
    /// <summary>Secteur de l'en-tête et du BAM.</summary>
    public const int HeaderSector = 0;
    /// <summary>Premier secteur de répertoire utilisé en repli.</summary>
    public const int DirectorySector = 1;
    /// <summary>Offset du nom du volume.</summary>
    public const int VolumeNameOffset = 0x90;
    /// <summary>Marqueur d'en-tête DOS.</summary>
    public const byte HeaderSignature = 0x41;
    /// <summary>Offset de la première entrée BAM.</summary>
    public const int BamEntriesOffset = 4;
    /// <summary>Taille d'une entrée BAM.</summary>
    public const int BamEntrySize = 4;
}
