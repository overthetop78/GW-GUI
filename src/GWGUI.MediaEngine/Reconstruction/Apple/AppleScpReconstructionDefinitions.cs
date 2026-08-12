namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>Définit les paramètres de sélection propres à la reconstruction des captures SCP Apple.</summary>
internal static class AppleScpReconstructionDefinitions
{
    /// <summary>Identité du reconstructeur Macintosh et Lisa dans les diagnostics de détection.</summary>
    public const string MacintoshReconstructorName = "Macintosh/Lisa";
    /// <summary>Identité du reconstructeur Apple II dans les diagnostics de détection.</summary>
    public const string AppleIIReconstructorName = "Apple II";
    /// <summary>Identité du reconstructeur Apple II ProDOS dans les diagnostics.</summary>
    public const string AppleIIProDosReconstructorName = "Apple II ProDOS";
    /// <summary>Identité du reconstructeur RWTS18 dans les diagnostics de détection.</summary>
    public const string Rwts18ReconstructorName = "RWTS18";
    /// <summary>Facteurs essayés autour de la durée de cellule Macintosh estimée.</summary>
    public static IReadOnlyList<double> MacintoshBitCellFactors { get; } = Array.AsReadOnly([1.0, 0.95, 1.05, 0.9, 1.1, 0.85, 1.15]);
    /// <summary>Poids d'un numéro de secteur distinct dans le score Macintosh.</summary>
    public const int DistinctSectorScoreWeight = 100;
    /// <summary>Poids d'un secteur intègre dans le score Macintosh.</summary>
    public const int ValidSectorScoreWeight = 10;
}
