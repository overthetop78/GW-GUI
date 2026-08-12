namespace GWGUI.MediaEngine.Geometries.Atari;

/// <summary>Définit les tailles et nombres de secteurs des géométries Atari 8 bits reconnues.</summary>
internal static class Atari8BitGeometry
{
    /// <summary>Première face d'une image Atari 8 bits.</summary>
    public const int FirstHead = 0;
    /// <summary>Premier numéro de secteur d'une image Atari 8 bits.</summary>
    public const int FirstSectorNumber = 1;
    /// <summary>Nombre de faces d'une image Atari 8 bits.</summary>
    public const int HeadCount = 1;
    /// <summary>Nombre de secteurs logiques par cylindre dans la vue linéaire d'une image ATR.</summary>
    public const int LinearSectorsPerCylinder = 1;
    /// <summary>Taille d'un secteur simple densité et d'un secteur d'amorçage, en octets.</summary>
    public const int SingleDensitySectorSize = 128;
    /// <summary>Taille d'un secteur double densité, en octets.</summary>
    public const int DoubleDensitySectorSize = 256;
    /// <summary>Nombre de secteurs d'amorçage conservant la taille simple densité.</summary>
    public const int BootSectorCount = 3;
    /// <summary>Nombre de secteurs par piste des formats 90 et 180 Kio.</summary>
    public const int StandardSectorsPerTrack = 18;
    /// <summary>Nombre de secteurs par piste du format 130 Kio.</summary>
    public const int EnhancedSectorsPerTrack = 26;
}
