namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Définit les critères de sélection automatique des familles ISO.</summary>
internal static class AutomaticIsoScpSelectionRules
{
    /// <summary>Taille sectorielle BBC DFS, en octets.</summary>
    public const int BbcSectorSize = 256;
    /// <summary>Nombre de secteurs par piste BBC DFS.</summary>
    public const int BbcSectorsPerTrack = 10;
    /// <summary>Taille sectorielle IBM analysée par le BPB, en octets.</summary>
    public const int IbmSectorSize = 512;
    /// <summary>Taille sectorielle Atari 8 bits simple densité, en octets.</summary>
    public const int AtariSingleDensitySectorSize = 128;
    /// <summary>Taille sectorielle Atari 8 bits double densité, en octets.</summary>
    public const int AtariDoubleDensitySectorSize = 256;
    /// <summary>Nombre standard de secteurs par piste Atari 8 bits.</summary>
    public const int AtariStandardSectorsPerTrack = 18;
    /// <summary>Nombre de secteurs par piste Atari 8 bits en densité améliorée.</summary>
    public const int AtariEnhancedSectorsPerTrack = 26;
    /// <summary>Score attribué à une intégrité valide.</summary>
    public const int ValidIntegrityScore = 2;
    /// <summary>Score attribué à une intégrité inconnue.</summary>
    public const int UnknownIntegrityScore = 1;
    /// <summary>Score attribué à une intégrité invalide.</summary>
    public const int InvalidIntegrityScore = 0;
}
