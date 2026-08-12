namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Décrit les drapeaux d'une entrée de répertoire RT-11.</summary>
[Flags]
public enum Rt11DirectoryEntryStatus : ushort
{
    /// <summary>Fichier provisoire.</summary>
    Tentative = 0x0100,
    /// <summary>Zone libre.</summary>
    Empty = 0x0200,
    /// <summary>Fichier permanent.</summary>
    Permanent = 0x0400,
    /// <summary>Fin du segment.</summary>
    EndOfSegment = 0x0800,
    /// <summary>Fichier protégé.</summary>
    Protected = 0x8000
}
